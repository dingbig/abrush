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

Shader "Brush/Special/SoftHighlighter" {
Properties {
  _MainTex ("Texture", 2D) = "white" {}

  _Opacity ("Opacity", Range(0, 1)) = 1
	_ClipStart("Clip Start", Float) = 0
	_ClipEnd("Clip End", Float) = -1
}

Category {
  Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
  Blend SrcAlpha One
  AlphaTest Greater .01
  ColorMask RGB
  Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

  SubShader {
    Pass {

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile __ AUDIO_REACTIVE
      #pragma multi_compile __ ODS_RENDER ODS_RENDER_CM
      #pragma multi_compile __ SELECTION_ON
      #include "UnityCG.cginc"
      #include "Assets/Shaders/Include/Brush.cginc"
      #include "Assets/Shaders/Include/MobileSelection.cginc"

      sampler2D _MainTex;
      uniform float _ClipStart;
      uniform float _ClipEnd;
      uniform float _Opacity;

      struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float3 normal : NORMAL;
        float2 texcoord : TEXCOORD0;
        uint id : SV_VertexID;
      };

      struct v2f {
        float4 pos : POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float2 id : TEXCOORD2;
      };

      float4 _MainTex_ST;

      v2f vert (appdata_t v)
      {
        PrepForOds(v.vertex);

        v2f o;

        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
#ifdef AUDIO_REACTIVE
        v.color = TbVertToSrgb(v.color);
        o.color = musicReactiveColor(v.color, _BeatOutput.z);
        v.vertex = musicReactiveAnimation(v.vertex, v.color, _BeatOutput.z, o.texcoord.x);
        o.color = SrgbToNative(o.color);
#else
        o.color = TbVertToNative(v.color);
#endif
        o.pos = UnityObjectToClipPos(v.vertex);
        o.id = (float2)v.id;
        return o;

      }

      fixed4 frag (v2f i) : COLOR
      {
        if (_ClipEnd > 0 && !(i.id.x > _ClipStart && i.id.y < _ClipEnd)) discard;


        float4 c = tex2D(_MainTex, i.texcoord );
        c *= i.color;
        c.a *= _Opacity;
        FRAG_MOBILESELECT(c)
        return c;
      }
      ENDCG
    }
  }
}
}
