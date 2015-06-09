using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks
{
    static class Utilities {
        public static Vector3 GetPosition(Transform trans) {
            //Transform targetTabTrans = complex.transform.Find("CrewPanels/panel_enlisted/tabs/tab_kia");
            //BTPanelTab targetTab = targetTabTrans.GetComponent<BTPanelTab>();
            var uiCams = UIManager.instance.uiCameras;
            EZCameraSettings uiCam = null;
            for( int i = 0; i < uiCams.Length; i++ ){
                if( (uiCams[i].mask & (1 << trans.gameObject.layer)) != 0 ){
                    uiCam = uiCams[i];
                    break;
                }
            }
            Vector3 screenPos = uiCam.camera.WorldToScreenPoint(trans.position);
            screenPos.y = Screen.height - screenPos.y;
            return screenPos;
        }


        public static List<string> EnumerateTransformDescendents(Transform trans) {
            List<string> children = new List<string>();
            if( trans == null ){
                return children;
            }

            foreach(Transform child in trans){
                string name = child.name;
                children.Add(name);
                List<string> grandchildren = EnumerateTransformDescendents(child);
                foreach( string grandchild in grandchildren ){
                    children.Add(name + "/" + grandchild);
                }
            }

            return children;
        }
    }
}
