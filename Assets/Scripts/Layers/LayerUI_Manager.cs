﻿// Copyright 2022 The Open Brush Authors
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using System;
using UnityEditor;

namespace TiltBrush.Layers
{
    public class LayerUI_Manager : MonoBehaviour
    {
        public delegate void OnActiveSceneChanged(GameObject widget);
        public static event OnActiveSceneChanged onActiveSceneChanged;

        [SerializeField] private LocalizedString m_MainLayerName;
        [SerializeField] private LocalizedString m_AdditionalLayerName;

        [SerializeField] public GameObject modeltrackWidget;

        public GameObject mainWidget;
        public List<GameObject> m_Widgets;
        public int scrollOffset = 0; 
        public float scrollHeight = 0.2f; // Height of each element in scroll zone
        private List<CanvasScript> m_Canvases;

        public Component animationUI_Manager;
        public GameObject layersWidget;


        public GameObject scrollUpButton;
        public GameObject scrollDownButton;
        

        private void Start()
        {
            ResetUI();
            initScroll();
            App.Scene.animationUI_manager.startTimeline();
        }


       
        private void ResetUI()
        {


            m_Canvases = new List<CanvasScript>();
            var layerCanvases = App.Scene.LayerCanvases.ToArray();


            foreach (GameObject widget in m_Widgets){

                Destroy(widget);
            }
            m_Widgets.Clear();

        
            int i=0;
            for (i = 0; i < layerCanvases.Length; i++)
            {
            var newWidget = Instantiate(layersWidget,this.gameObject.transform,false);
            // newWidget.transform.SetParent(this.gameObject.transform, false);
        


            if (i == 0) newWidget.GetComponentInChildren<DeleteLayerButton>()?.gameObject.SetActive(false);
            if (i == 0) newWidget.GetComponentInChildren<SquashLayerButton>()?.gameObject.SetActive(false);

            if (layerCanvases[i] == App.ActiveCanvas){
                print("ACTIVE CANVAS");
                print(layerCanvases[i]);
                newWidget.GetComponentInChildren<FocusLayerButton>().SetButtonActivation(layerCanvases[i] == App.ActiveCanvas);
            }
            newWidget.GetComponentInChildren<TMPro.TextMeshPro>().text = (i == 0) ? $"{m_MainLayerName.GetLocalizedString()}" : $"{m_AdditionalLayerName.GetLocalizedString()} {i}";
                // Active button means hidden layer
            newWidget.GetComponentInChildren<ToggleVisibilityLayerButton>().SetButtonActivation(!layerCanvases[i].isActiveAndEnabled);
             
            Vector3 localPos = mainWidget.transform.localPosition;
            localPos.y -= i*scrollHeight;

            print("LOCAL POS BEFORE " +  localPos.y + " " + scrollOffset);

            localPos.y -= scrollOffset;
            print("LOCAL POS AFTER " +  localPos.y);

            newWidget.transform.localPosition = localPos;
            m_Widgets.Add(newWidget);

            m_Canvases.Add(layerCanvases[i]);

            
            
            }
            if (this.gameObject.GetComponent<TiltBrush.FrameAnimation.AnimationUI_Manager>() != null){
                
                if (this.gameObject.GetComponent<TiltBrush.FrameAnimation.AnimationUI_Manager>().animatedModels != null){

                    var animatedModels = this.gameObject.GetComponent<TiltBrush.FrameAnimation.AnimationUI_Manager>().animatedModels;
                    print(animatedModels.Count);

                    for (int a = 0; a < animatedModels.Count; a++){

                    var newWidget = Instantiate(modeltrackWidget,this.gameObject.transform,false);

                    var modelPreview = Instantiate(animatedModels[a].gameObject.GetComponentInChildren<ObjModelScript>().gameObject,newWidget.gameObject.transform,false);
                     
                    modelPreview.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
                    modelPreview.transform.localPosition = new Vector3(-0.785000026f,-0.0680000037f,-0.141000003f);
                     EditorGUIUtility.PingObject(modelPreview);

                    newWidget.GetComponentInChildren<TMPro.TextMeshPro>().text = animatedModels[a].name;
                    // Active button means hidden layer
                    newWidget.GetComponentInChildren<FocusModelTrackButton>().SetButtonActivation(false);
                    Vector3 localPos = mainWidget.transform.localPosition;
                    localPos.y -=  (a + i)*scrollHeight;

                    newWidget.transform.localPosition = localPos;
                    m_Widgets.Add(newWidget);
              
                }
                }
            }
            

            UpdateScroll();
            // print("START RESET UI");
            // m_Canvases = new List<CanvasScript>();
            // var canvases = App.Scene.LayerCanvases.ToArray();
            // for (int i = 0; i < m_Widgets.Count; i++)
            // {
            //     print("FOR HERE " + i + " " + canvases.Length);
            //     var widget = m_Widgets[i];

             
            //     if (i >= canvases.Length)
            //     {
                    
            //         widget.SetActive(false);
            //         continue;
            //     }
            //     // widget.SetActive(true);
            //      widget.SetActive(false);
            //     var canvas = canvases[i];
            //     if (i == 0) widget.GetComponentInChildren<DeleteLayerButton>()?.gameObject.SetActive(false);
            //     if (i == 0) widget.GetComponentInChildren<SquashLayerButton>()?.gameObject.SetActive(false);
            //     widget.GetComponentInChildren<FocusLayerButton>().SetButtonActivation(canvas == App.ActiveCanvas);
            //     print("BEFORE PRE STRING");
            //     print(" PRE STRINGS 1" + m_MainLayerName.GetLocalizedString() );
            //      print(" PRE STRINGS 2" +  m_AdditionalLayerName.GetLocalizedString());
            //     widget.GetComponentInChildren<TMPro.TextMeshPro>().text = (i == 0) ? $"{m_MainLayerName.GetLocalizedString()}" : $"{m_AdditionalLayerName.GetLocalizedString()} {i}";
            //     // Active button means hidden layer
            //     widget.GetComponentInChildren<ToggleVisibilityLayerButton>().SetButtonActivation(!canvas.isActiveAndEnabled);
            //     m_Canvases.Add(canvas);
            // }

            // print("RESET UI DONE");
            // print(m_Canvases.Count);
        }

        private void initScroll(){
            scrollOffset = 0;
            scrollHeight =0.2f;
        }
        private void UpdateScroll(){

            print("SCROLL OFFSET " + scrollOffset);
            print("WIDGET COUNT " +  m_Widgets.Count);
             for (int i = 0; i < m_Widgets.Count; i++)
            {
                Vector3 localPos = mainWidget.transform.localPosition;
                float subtractingVal = i*scrollHeight + scrollOffset*scrollHeight;
                localPos.y -= subtractingVal;


                m_Widgets[i].transform.localPosition = localPos;

                int thisWidgetOffset = i + scrollOffset;

                print("WIDGET OFFSET " +  thisWidgetOffset);
                if (thisWidgetOffset >= 7 || thisWidgetOffset < 0){
                    m_Widgets[i].SetActive(false);
                }else{
                     m_Widgets[i].SetActive(true);
                }
            }

            scrollUpButton.SetActive(scrollOffset != 0);
            scrollDownButton.SetActive(scrollOffset + m_Widgets.Count  > 7);

            App.Scene.animationUI_manager.updateTrackScroll(scrollOffset,scrollHeight);

        }
        public void scrollDirection(bool upDirection){
            if (scrollOffset == 0 && upDirection) return;
            if (scrollOffset + m_Widgets.Count  <= 7 && !upDirection) return;
            scrollOffset += (Convert.ToInt32(upDirection)*2 - 1);
            UpdateScroll();
        }
        private void OnLayerCanvasesUpdate()
        {
            ResetUI();
        }

        // Subscribes to events
        private void OnEnable()
        {
            App.Scene.ActiveCanvasChanged += ActiveSceneChanged;
            App.Scene.LayerCanvasesUpdate += OnLayerCanvasesUpdate;
        }

        // Unsubscribes to events
        private void OnDisable()
        {
            App.Scene.ActiveCanvasChanged -= ActiveSceneChanged;
            App.Scene.LayerCanvasesUpdate -= OnLayerCanvasesUpdate;
        }

        public void DeleteLayer(GameObject widget)
        {
            if (GetCanvasFromWidget(widget) == App.Scene.MainCanvas) return; // Don't delete the main canvas
            var layer = GetCanvasFromWidget(widget);
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(new DeleteLayerCommand(layer));
        }
        public void DeleteLayerGeneral(){
            if ( App.Scene.ActiveCanvas == App.Scene.MainCanvas) return; // Don't delete the main canvas
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(new DeleteLayerCommand(App.Scene.ActiveCanvas));
            App.Scene.animationUI_manager.resetTimeline();
        }

        public void SquashLayer(GameObject widget)
        {
            var canvas = GetCanvasFromWidget(widget);
            var index = m_Widgets.IndexOf(widget);

            print("SQUASHING ORIG" + index);

            var prevCanvas = m_Canvases[Mathf.Max(index - 1, 0)];
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(
                new SquashLayerCommand(canvas, prevCanvas)
            );
        }

          public void SquashLayerGeneral()
        {

            var canvas = App.Scene.ActiveCanvas;
            var index = App.Scene.GetLayerNumFromCanvas(App.Scene.ActiveCanvas);
            print("SQUASHING GENERAL" + index);
            var prevCanvas = App.Scene.GetCanvasFromLayerNum(Mathf.Max(index -1, 0));
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(
                new SquashLayerCommand(canvas, prevCanvas)
            );
        }

        public void ClearLayerContents(GameObject widget)
        {
            CanvasScript canvas = GetCanvasFromWidget(widget);
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(new ClearLayerCommand(canvas));
        }

        public void ClearLayerContentsGeneral()
        {
            CanvasScript canvas = App.Scene.ActiveCanvas;
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(new ClearLayerCommand(canvas));
        }

        public void AddLayer()
        {
            print("ADD 1~ LAYER NOW ");
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(new AddLayerCommand(true));
            print("ADD 2~  ");
            App.Scene.animationUI_manager.resetTimeline();
        }

        public void ToggleVisibility(GameObject widget)
        {
            CanvasScript canvas = GetCanvasFromWidget(widget);
            print("TOGGLE VISIBILITY");
            print(canvas);
            App.Scene.ToggleLayerVisibility(canvas);
        }

        public void SetActiveLayer(GameObject widget)
        {
            print("SETTING ACTIVE LAYER");
            var newActiveCanvas = GetCanvasFromWidget(widget);
            print(newActiveCanvas);
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(new ActivateLayerCommand(newActiveCanvas));
            ResetUI();
        }

        private void ActiveSceneChanged(CanvasScript prev, CanvasScript current)
        {
            onActiveSceneChanged?.Invoke(GetWidgetFromCanvas(current));
        }

        private CanvasScript GetCanvasFromWidget(GameObject widget)
        {
            return m_Canvases[m_Widgets.IndexOf(widget)];
        }

        private GameObject GetWidgetFromCanvas(CanvasScript canvas)
        {
            var index = m_Canvases.IndexOf(canvas);
            return index >= 0 ? m_Widgets[index] : null;
        }

        public void HandleCopySelectionToCurrentLayer()
        {
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(
                new DuplicateSelectionCommand(SelectionManager.m_Instance.SelectionTransform)
            );
        }
    }
}
