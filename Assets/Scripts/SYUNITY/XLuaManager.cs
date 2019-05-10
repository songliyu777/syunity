using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace SYUNITY
{
    [LuaCallCSharp]
    public class XLuaManager : SYSingleton<XLuaManager>, IDisposable
    {
        public LuaEnv luaEnv
        {
            private set;
            get;
        }

        public XLuaManager()
        {
            luaEnv = new LuaEnv();
            LuaEnv.CustomLoader loader = OriginalLuaLoader;
            luaEnv.AddLoader(loader);
        }

        // 从项目中加载原始的Lua脚本，仅在editor模式下执行
        byte[] OriginalLuaLoader(ref string luaFileName)
        {
            if (string.IsNullOrEmpty(luaFileName))
            {
                return null;
            }

            //lua文件的存放路径
            luaFileName += ".lua";
            string folder = string.Format("{0}/NetFiles/Resources", Application.dataPath);
            string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
            string rightFile = null;
            foreach (string f in files)
            {
                string n = Path.GetFileNameWithoutExtension(f);
                if (n == luaFileName)
                {
                    rightFile = f;
                    break;
                }
            }

            if (string.IsNullOrEmpty(rightFile))
            {
                return null;
            }

            luaFileName = rightFile;
            return File.ReadAllBytes(rightFile);
        }

        // void Tick()： 清除Lua的未手动释放的LuaBase（比如，LuaTable， LuaFunction），以及其它一些事情。需要定期调用，比如在MonoBehaviour的Update中调用。
        public void Update()
        {
            if (luaEnv != null)
            {
                luaEnv.Tick();
            }
        }

        /// <summary>
        /// 加载lua表
        /// </summary>
        /// <param name="name">lua表名，注意：表名与lua文件名必须一致</param>
        public void LoadLuaTable(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("string.IsNullOrEmpty(name)");
                return;
            }
            string code = string.Format("require '{0}'", name);
            luaEnv.DoString(code);
        }

        /// <summary>
        /// 获取lua表
        /// </summary>
        /// <param name="name">lua表名，注意：表名与lua文件名必须一致</param>
        public LuaTable GetLuaTable(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("string.IsNullOrEmpty(name)");
                return null;
            }

            LuaTable table = luaEnv.Global.Get<LuaTable>(name);
            if (table == null)
            {
                LoadLuaTable(name);
                table = luaEnv.Global.Get<LuaTable>(name);
            }
            return table;
        }

        public void Dispose()
        {
            if (luaEnv != null)
            {
                luaEnv.GC();
                luaEnv.Dispose();
            }

            luaEnv = null;
            GC.SuppressFinalize(this);
        }
    }
}