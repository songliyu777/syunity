using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace SYUNITY
{
    [LuaCallCSharp]
    public class LuaMonoBehaviour : MonoBehaviour
    {
        public string luaScriptName;

        //调用频繁的函数采用
        Action m_updateFunc;
        Action m_lateUpdateFunc;

        public LuaTable luaTable
        {
            get;
            private set;
        }

        public bool Load()
        {
            if (string.IsNullOrEmpty(luaScriptName))
            {
                return false;
            }

            luaTable = XLuaManager.Ins.GetLuaTable(luaScriptName);
            if (luaTable == null)
            {
                return false;
            }

            luaTable.Set<string, Transform>("transform", transform);
            luaTable.Set<string, GameObject>("gameObject", gameObject);

            m_updateFunc = luaTable.Get<Action>("Update");
            m_lateUpdateFunc = luaTable.Get<Action>("LateUpdate");

            return true;
        }

        void CallLuaFunction(string funcName)
        {
            if (string.IsNullOrEmpty(funcName))
            {
                Debug.LogError("argument error: funcName");
                return;
            }
            Action func = luaTable.Get<Action>(funcName);
            if (func != null)
            {
                func();
            }
        }

        void Awake()
        {
            if (Load())
            {
                CallLuaFunction("Awake");
            }
            else
            {
                // 如果 Name 为空，可能是 Add component
                if (!string.IsNullOrEmpty(luaScriptName))
                {
                    Debug.LogError("Load lua table failed, no table in " + luaScriptName);
                    return;
                }
            }
        }

        void Start()
        {
            // 此处应为 Add component 的情况，导致Tabel=null
            // 可在Add component和赋值luaComponentName后先主动调用一次Load方法
            if (luaTable == null)
            {
                if (string.IsNullOrEmpty(luaScriptName))
                {
                    Debug.LogError("string.IsNullOrEmpty(ComponentName)");
                    return;
                }

                if (!Load())
                {
                    Debug.LogError("Load lua table failed, no table in " + luaScriptName);
                    return;
                }
            }
            CallLuaFunction("Start");
        }

        void Update()
        {
            if (m_updateFunc != null)
            {
                m_updateFunc();
            }
        }

        void LateUpdate()
        {
            if (m_lateUpdateFunc != null)
            {
                m_lateUpdateFunc();
            }
        }

        void OnEnable()
        {
            CallLuaFunction("OnEnable");
        }

        void OnDisable()
        {
            CallLuaFunction("OnDisable");
        }

        void OnDestroy()
        {
            CallLuaFunction("OnDestroy");

            luaTable.Set<string, Transform>("transform", null);
            luaTable.Set<string, GameObject>("gameObject", null);

            m_updateFunc = null;
            m_lateUpdateFunc = null;

            luaTable.Dispose();
            luaTable = null;
        }

        public static class LuaMonoBehaviourConfig
        {
            [CSharpCallLua]
            public static List<Type> CSharpCallLua = new List<Type>()
            {
                typeof(Action)
            };
        }
    }
}

