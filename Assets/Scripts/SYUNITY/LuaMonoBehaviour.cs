using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace SYUNITY
{
    [System.Serializable]
    public class Injection
    {
        public string name;
        public GameObject value;
    }

    [LuaCallCSharp]
    public class LuaMonoBehaviour : MonoBehaviour
    {
        public string luaScriptName;
        //用于editor填入值使用
        public Injection[] injections;
        //调用频繁的函数采用
        Action<LuaTable> m_updateFunc;
        Action<LuaTable> m_lateUpdateFunc;
        Action<LuaTable> m_fixedUpdateFunc;

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

            if (injections != null)
            {
                foreach (var injection in injections)
                {
                    luaTable.Set(injection.name, injection.value);
                }
            }

            luaTable.Set<string, Transform>("transform", transform);
            luaTable.Set<string, GameObject>("gameObject", gameObject);

            m_updateFunc = luaTable.Get<Action<LuaTable>>("Update");
            m_lateUpdateFunc = luaTable.Get<Action<LuaTable>>("LateUpdate");
            m_fixedUpdateFunc = luaTable.Get<Action<LuaTable>>("FixedUpdate");

            return true;
        }

        void CallLuaFunction(string funcName)
        {
            if (string.IsNullOrEmpty(funcName))
            {
                Debug.LogError("argument error:" + funcName);
                return;
            }
            if (luaTable == null)
            {
                Debug.LogError("table error:" + luaScriptName);
                return;
            }
            Action<LuaTable> func = luaTable.Get<Action<LuaTable>>(funcName);
            if (func != null)
            {
                func(luaTable);
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
                m_updateFunc(luaTable);
            }
        }

        void LateUpdate()
        {
            if (m_lateUpdateFunc != null)
            {
                m_lateUpdateFunc(luaTable);
            }
        }

        void FixedUpdate()
        {
            if(m_fixedUpdateFunc != null)
            {
                m_fixedUpdateFunc(luaTable);
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
            if (luaTable == null)
            {
                return;
            }
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

