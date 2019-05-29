using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using XLua;

namespace SYUNITY
{
    [LuaCallCSharp]
    public class UIManager : LuaMonoBehaviour
    {
        public void AddPackage(string packageName)
        {
            UIPackage.AddPackage(packageName, LoadResource);
        }

        public object LoadResource(string name, string extension, System.Type type, out DestroyMethod destroyMethod)
        {
            destroyMethod = DestroyMethod.None;
            Debug.LogWarning(name + ":" + extension + ":" + type);
            return null;
        }

        public static class UIManagerConfig
        {
            [CSharpCallLua]
            public static List<Type> CSharpCallLua = new List<Type>()
            {
                typeof(UIManager)
            };
        }
    }
}
