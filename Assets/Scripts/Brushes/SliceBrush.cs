// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

namespace TiltBrush
{

    /// SliceBrush gives each control point a quad whose normal is the direction
    /// of motion. The quad has uses 3 texture coordinates. U and V have the
    /// standard mapping from (0,0) to (1,1). W is the length in meters along the
    /// stroke.
    ///
    /// The curve ignores pointer orientation (except for the first knot); it
    /// is framed to minimize twist.
    class SliceBrush : GeometryBrush
    {
        const float M2U = App.METERS_TO_UNITS;
        const float U2M = App.UNITS_TO_METERS;

        const float TWOPI = 2 * Mathf.PI;

        const float kMinimumMove_PS = 5e-4f * M2U;
        const ushort kVertsInQuad = 4;

        const float kSolidMinLengthMeters_PS = 0.0001f;
        const float kSolidAspectRatio = 0.2f;

        public SliceBrush()
            : base(bCanBatch: true,
                upperBoundVertsPerKnot: kVertsInQuad * 2,
                bDoubleSided: true)
        {
        }

        //
        // GeometryBrush API
        //

        protected override void InitBrush(BrushDescriptor desc, TrTransform localPointerXf)
        {
            base.InitBrush(desc, localPointerXf);
            m_geometry.Layout = GetVertexLayout(desc);
        }

        override public GeometryPool.VertexLayout GetVertexLayout(BrushDescriptor desc)
        {
            return new GeometryPool.VertexLayout
            {
                uv0Size = 3,
                bUseNormals = true,
                bUseColors = true,
                bUseTangents = false
            };
        }

        override public float GetSpawnInterval(float pressure01)
        {
            return kSolidMinLengthMeters_PS * App.METERS_TO_UNITS * POINTER_TO_LOCAL +
                (PressuredSize(pressure01) * kSolidAspectRatio);
        }

        override protected void ControlPointsChanged(int iKnot0)
        {
            // Updating a control point affects geometry generated by previous knot
            // (if there is any). The HasGeometry check is not a micro-optimization:
            // it also keeps us from backing up past knot 0.
            int start = (m_knots[iKnot0 - 1].HasGeometry) ? iKnot0 - 1 : iKnot0;

            // Frames knots, determines how much geometry each knot should get
            OnChanged_FrameKnots(start);
            OnChanged_MakeGeometry(start);
            ResizeGeometry();
        }

        // This approximates parallel transport.
        static Quaternion ComputeMinimalRotationFrame(
            Vector3 nTangent, Quaternion qPrevFrame)
        {
            Vector3 nPrevTangent = qPrevFrame * Vector3.forward;
            Quaternion minimal = Quaternion.FromToRotation(nPrevTangent, nTangent);
            return minimal * qPrevFrame;
        }

        // Fills in any knot data needed for geometry generation.
        // - fill in length, qFrame
        // - calculate strip-break points
        void OnChanged_FrameKnots(int iKnot0)
        {
            Knot prev = m_knots[iKnot0 - 1];
            for (int iKnot = iKnot0; iKnot < m_knots.Count; ++iKnot)
            {
                Knot cur = m_knots[iKnot];

                bool shouldBreak = false;

                Vector3 vMove = cur.point.m_Pos - prev.point.m_Pos;
                cur.length = vMove.magnitude;

                if (cur.length < kMinimumMove_PS * POINTER_TO_LOCAL)
                {
                    shouldBreak = true;
                }
                else
                {
                    Vector3 nTangent = vMove / cur.length;
                    if (prev.HasGeometry)
                    {
                        cur.qFrame = ComputeMinimalRotationFrame(nTangent, prev.qFrame);
                    }
                    else
                    {
                        Vector3 nRight, nUp;
                        // No previous orientation; compute a reasonable starting point
                        ComputeSurfaceFrameNew(Vector3.zero, nTangent, cur.point.m_Orient, out nRight, out nUp);
                        cur.qFrame = Quaternion.LookRotation(nTangent, nUp);
                    }
                }

                if (shouldBreak)
                {
                    cur.qFrame = new Quaternion(0, 0, 0, 0);
                }

                // Just mark whether or not the strip is broken
                // tri/vert allocation will happen next pass
                cur.nTri = cur.nVert = (ushort)(shouldBreak ? 0 : 1);
                m_knots[iKnot] = cur;
                prev = cur;
            }
        }

        // Textures are laid out so u goes along the strip,
        // and v goes across the strip (from left to right)
        void OnChanged_MakeGeometry(int iKnot0)
        {
            // Invariant: there is a previous knot.
            Knot prev = m_knots[iKnot0 - 1];
            for (int iKnot = iKnot0; iKnot < m_knots.Count; ++iKnot)
            {
                // Invariant: all of prev's geometry (if any) is correct and up-to-date.
                // Thus, there is no need to modify anything shared with prev.
                Knot cur = m_knots[iKnot];

                cur.iTri = prev.iTri + prev.nTri;
                cur.iVert = (ushort)(prev.iVert + prev.nVert);

                if (cur.HasGeometry)
                {
                    cur.nVert = cur.nTri = 0;

                    Vector3 rt = cur.qFrame * Vector3.right;
                    Vector3 up = cur.qFrame * Vector3.up;
                    Vector3 fwd = cur.qFrame * Vector3.forward;

                    bool isStart = !prev.HasGeometry;

                    // Verts, back half

                    float w0;
                    if (isStart)
                    {
                        float halfSize = PressuredSize(prev.smoothedPressure) * .5f;
                        w0 = 0;
                        MakeQuad(ref cur, prev.point.m_Pos, halfSize, up, rt, fwd, w0);
                    }
                    else
                    {
                        cur.iVert -= 4;
                        cur.nVert += 4;
                        w0 = m_geometry.m_Texcoord0.v3[cur.iVert].z;
                    }

                    // Verts, front half

                    {
                        float halfSize = PressuredSize(cur.smoothedPressure) * .5f;
                        float w1 = w0 + cur.length * U2M;
                        MakeQuad(ref cur, cur.point.m_Pos, halfSize, up, rt, fwd, w1);
                    }
                }

                m_knots[iKnot] = cur;
                prev = cur;
            }
        }

        void MakeQuad(
            ref Knot k, Vector3 center, float halfSize,
            Vector3 up, Vector3 rt, Vector3 fwd, float w)
        {
            up *= halfSize;
            rt *= halfSize;

            // Clockwise looking down the knots
            // tangent is in u direction
            AppendVert(ref k, center - rt - up, fwd, rt, 0, 0, w);
            AppendVert(ref k, center - rt + up, fwd, rt, 0, 1, w);
            AppendVert(ref k, center + rt + up, fwd, rt, 1, 1, w);
            AppendVert(ref k, center + rt - up, fwd, rt, 1, 0, w);
            AppendTri(ref k, 0, 1, 2);
            AppendTri(ref k, 2, 3, 0);
        }

        /// Resizes arrays if necessary, appends data, mutates knot's vtx count
        void AppendVert(ref Knot k, Vector3 pos, Vector3 n,
                        Vector3 tan, float u, float v, float w)
        {
            var color = CalcColor(m_Color, k.point);
            color.a = 255;
            // Vector4 tan4 = tan;
            // tan4.w = 1;
            Vector3 uvw = new Vector3(u, v, w);

            int i = k.iVert + k.nVert++;
            if (i == m_geometry.m_Vertices.Count)
            {
                m_geometry.m_Vertices.Add(pos);
                m_geometry.m_Colors.Add(color);
                m_geometry.m_Normals.Add(n);
                //m_geometry.m_Tangents .Add(tan4);
                m_geometry.m_Texcoord0.v3.Add(uvw);
            }
            else
            {
                m_geometry.m_Vertices[i] = pos;
                m_geometry.m_Colors[i] = color;
                m_geometry.m_Normals[i] = n;
                //m_geometry.m_Tangents[i] = tan4;
                m_geometry.m_Texcoord0.v3[i] = uvw;
            }
        }

        void AppendTri(ref Knot k, int t0, int t1, int t2)
        {
            int i = (k.iTri + k.nTri++) * 3;
            if (i == m_geometry.m_Tris.Count)
            {
                m_geometry.m_Tris.Add(k.iVert + t0);
                m_geometry.m_Tris.Add(k.iVert + t2);
                m_geometry.m_Tris.Add(k.iVert + t1);
            }
            else
            {
                m_geometry.m_Tris[i + 0] = k.iVert + t0;
                m_geometry.m_Tris[i + 1] = k.iVert + t2;
                m_geometry.m_Tris[i + 2] = k.iVert + t1;
            }
        }

        bool IsPenultimate(int iKnot)
        {
            return (iKnot + 1 == m_knots.Count || !m_knots[iKnot + 1].HasGeometry);
        }
    }
} // namespace TiltBrush
