using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using XLua;
using XAsset;

namespace SYUNITY
{
    [LuaCallCSharp]
    public class UIManager : LuaMonoBehaviour
    {
        class UIAsset
        {
            public List<Asset> lstAsset = new List<Asset>();
            int referenceCount = 0;
            public void AddRef() { referenceCount++; }
            public void Drop()
            {
                referenceCount--;
                if (referenceCount == 0)
                {
                    foreach(Asset a in lstAsset)
                    {
                        a.Release();
                    }
                }
            }
            public void AddAsset(Asset asset)
            {
                lstAsset.Add(asset);
            }
        }

        private static string uiPath = "Assets/NetRes/Resources/UI/";

        static readonly Dictionary<string, UIAsset> assets = new Dictionary<string, UIAsset>();
        public void AddPackage(string packageName)
        {
            if (!assets.ContainsKey(packageName))
            {
                assets[packageName] = new UIAsset();
            }
            assets[packageName].AddRef();
            UIPackage.AddPackage(packageName, LoadResource);
        }
        public void RemovePackage(string packageName)
        {
            UIPackage.RemovePackage(packageName);
            if (assets.ContainsKey(packageName))
            {
                UIAsset uiAsset = assets[packageName];
                uiAsset.Drop();
            }
            else
            {
                Debug.LogError("no package:" + packageName);
            }
        }

        public object LoadResource(string name, string extension, System.Type type, out DestroyMethod destroyMethod)
        {
            string assetName = uiPath + name + extension;
            destroyMethod = DestroyMethod.None;
            string packageName = name.Substring(0, name.LastIndexOf('_'));
            if (!assets.ContainsKey(packageName))
            {
                Debug.LogError("LoadResource No Package:" + packageName);
                return null;
            }

#if UNITY_EDITOR
            Asset asset = Assets.Load(assetName, type);
            if (asset != null)
            {
                assets[packageName].AddAsset(asset);
                return asset.asset;
            }
#else
            if (Assets.ContainsAsset(assetName))
            {
                Asset asset = Assets.Load(assetName, type);
                if (asset != null)
                {
                    assets[packageName].AddAsset(asset);
                    return asset.asset;
                }
            }
#endif
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
