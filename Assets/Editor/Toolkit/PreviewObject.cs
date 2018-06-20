using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolkit
{
    class PreviewObject
    {
        public Texture texture { get; set; }
		public UnityEngine.Object gameObject { get; set; }

        public GUIContent content { get; set; }

        public bool loaded { get; set; }

		public PreviewObject(UnityEngine.Object obj , Texture tex)
        {
            texture = tex;
            gameObject = obj;
            loaded = false;
        }

    }
}

