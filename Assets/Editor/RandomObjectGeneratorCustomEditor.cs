using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Boids.Editor
{
    [CustomEditor(typeof(RandomObjectGenerator))]
    [CanEditMultipleObjects]
    public class RandomObjectGeneratorCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                foreach(var obj in targets)
                {
                    (obj as RandomObjectGenerator).Generate();
                }
            }
        }
    }
}
