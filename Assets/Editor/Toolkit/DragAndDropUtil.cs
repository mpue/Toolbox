using UnityEngine;
using UnityEditor;
using System;

namespace Toolkit
{
    public class DragAndDropUtil
    {

        /// <summary>
        /// Registers fieldRect for drop operations.
        /// sets the fieldValue to whatever that's been dropped, returns fieldValue in either cases.
        /// (could have used a ref parameter instead of returning the field)
        /// </summary>
        public static T RegisterFieldForDrop<T>(Rect fieldRect, T fieldValue, Func<UnityEngine.Object[], UnityEngine.Object> getDroppedObject) where T : UnityEngine.Object
        {
            Event e = Event.current;
            EventType eType = e.type;
            if (fieldRect.Contains(e.mousePosition) && eType == EventType.DragUpdated || eType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (eType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    fieldValue = getDroppedObject(DragAndDrop.objectReferences) as T;
                    Event.current.Use();
                }
            }
            return fieldValue;
        }
        /// <summary>
        /// Registers fieldRect for drag operations. dragObject is what's being dragged out of that field.
        /// </summary>
        public static void RegisterFieldForDrag(Rect fieldRect, UnityEngine.Object dragObject)
        {
            if (dragObject == null) return; // can't drag a null wtf!
            Event e = Event.current;
            if (fieldRect.Contains(e.mousePosition) && e.type == EventType.MouseDrag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { dragObject };
                DragAndDrop.StartDrag("drag");
                Event.current.Use();
            }
        }

        public static void DragDropArea<T>(string label, Action<T> onDrop, Action onMouseUp, float h = 25) where T : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            // draw a box area for our drag-drop
            GUILayout.Space(15f);
            Rect dropArea = GUILayoutUtility.GetRect(0, h);
            GUI.Box(dropArea, label);
            // cache the current event
            Event currentEvent = Event.current;
            // if our mouse isn't contained within that box area, exit out
            if (!dropArea.Contains(currentEvent.mousePosition)) return;
            if (onMouseUp != null)
                if (currentEvent.type == EventType.MouseUp)
                    onMouseUp();
            if (onDrop != null)
            {
                if (currentEvent.type == EventType.DragUpdated ||
                    currentEvent.type == EventType.DragPerform)
                {
                    // set the visual mode to copy
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    // if we dropped something
                    if (currentEvent.type == EventType.DragPerform)
                    {
                        // register that this drag-drop event has been handled by this control
                        DragAndDrop.AcceptDrag();
                        // loop over the dropped items
                        foreach (var item in DragAndDrop.objectReferences)
                        {
                            onDrop(item as T);
                        }
                    }
                    // since we've used the DragPerform event, we'll mark it as used
                    // (its type will change to EventType.Used)
                    // so that other controls ignore it
                    Event.current.Use();
                }
            }
            GUILayout.EndHorizontal();            
        }
    }
}
