﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace launcher
{
    public static class LaunchParameters
    {
        public enum eMode
        {
            HOST,
            SERVER,
            CLIENT
        }

        private static void AppendParameter(ref string svParameters, string parameter, string value = "")
        {
            svParameters += value == "" ? $"{parameter} " : $"{parameter} {value} ";
        }

        private static void AppendHostParameters(ref string svParameters)
        {
            if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.HostName)))
            {
                AppendParameter(ref svParameters, "+hostname", (string)Ini.Get(Ini.Vars.HostName));
                AppendParameter(ref svParameters, "+sv_pylonVisibility", (string)Ini.Get(Ini.Vars.Visibility).ToString());
            }
        }

        private static void AppendVideoParameters(ref string svParameters)
        {
            if ((bool)Ini.Get(Ini.Vars.Windowed))
                AppendParameter(ref svParameters, "-windowed");
            else
                AppendParameter(ref svParameters, "-fullscreen");

            if ((bool)Ini.Get(Ini.Vars.Borderless))
                AppendParameter(ref svParameters, "-noborder");
            else
                AppendParameter(ref svParameters, "-forceborder");

            AppendParameter(ref svParameters, "+fps_max", (string)Ini.Get(Ini.Vars.Max_FPS));

            if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Resolution_Width)))
                AppendParameter(ref svParameters, "-w", (string)Ini.Get(Ini.Vars.Resolution_Width));

            if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Resolution_Height)))
                AppendParameter(ref svParameters, "-h", (string)Ini.Get(Ini.Vars.Resolution_Height));
        }

        private static void AppendProcessorParameters(ref string svParameters)
        {
            int nReservedCores = int.Parse((string)Ini.Get(Ini.Vars.Reserved_Cores));
            if (nReservedCores > -1) // A reserved core count of 0 seems to crash the game on some systems.
                AppendParameter(ref svParameters, "-numreservedcores", (string)Ini.Get(Ini.Vars.Reserved_Cores));

            int nWorkerThreads = int.Parse((string)Ini.Get(Ini.Vars.Worker_Threads));
            if (nWorkerThreads > -1)
                AppendParameter(ref svParameters, "-numworkerthreads", (string)Ini.Get(Ini.Vars.Worker_Threads));
        }

        private static void AppendNetParameters(ref string svParameters)
        {
            AppendParameter(ref svParameters, "+net_encryptionEnable", (bool)Ini.Get(Ini.Vars.Encrypt_Packets) == true ? "1" : "0");
            AppendParameter(ref svParameters, "+net_useRandomKey", (bool)Ini.Get(Ini.Vars.Random_Netkey) == true ? "1" : "0");
            AppendParameter(ref svParameters, "+net_queued_packet_thread", (bool)Ini.Get(Ini.Vars.Queued_Packets) == true ? "1" : "0");

            if ((bool)Ini.Get(Ini.Vars.No_Timeout))
                AppendParameter(ref svParameters, "-notimeout");
        }

        private static void AppendConsoleParameters(ref string svParameters)
        {
            eMode mode = (eMode)(int)Ini.Get(Ini.Vars.Mode);

            if ((bool)Ini.Get(Ini.Vars.Show_Console) || mode == eMode.SERVER)
                AppendParameter(ref svParameters, "-wconsole");
            else
                AppendParameter(ref svParameters, "-noconsole");

            if ((bool)Ini.Get(Ini.Vars.Color_Console))
                AppendParameter(ref svParameters, "-ansicolor");

            if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Playlists_File)))
                AppendParameter(ref svParameters, "-playlistfile", (string)Ini.Get(Ini.Vars.Playlists_File));
        }

        public static string BuildParameter()
        {
            string svParameters = "";

            AppendProcessorParameters(ref svParameters);
            AppendConsoleParameters(ref svParameters);
            AppendNetParameters(ref svParameters);

            eMode mode = (eMode)(int)Ini.Get(Ini.Vars.Mode);
            switch (mode)
            {
                case eMode.HOST:
                    {
                        // GAME ###############################################################
                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Map)))
                            AppendParameter(ref svParameters, "+map", (string)Ini.Get(Ini.Vars.Map));

                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Playlist)))
                            AppendParameter(ref svParameters, "+launchplaylist", (string)Ini.Get(Ini.Vars.Playlist));

                        if ((bool)Ini.Get(Ini.Vars.Enable_Developer))
                        {
                            AppendParameter(ref svParameters, "-dev");
                            AppendParameter(ref svParameters, "-devsdk");
                        }

                        if ((bool)Ini.Get(Ini.Vars.Enable_Cheats))
                        {
                            AppendParameter(ref svParameters, "-dev");
                            AppendParameter(ref svParameters, "-showdevmenu");
                        }

                        // ENGINE ###############################################################
                        if ((bool)Ini.Get(Ini.Vars.No_Async))
                        {
                            AppendParameter(ref svParameters, "-noasync");
                            AppendParameter(ref svParameters, "+async_serialize", "0");
                            AppendParameter(ref svParameters, "+buildcubemaps_async", "0");
                            AppendParameter(ref svParameters, "+sv_asyncAIInit", "0");
                            AppendParameter(ref svParameters, "+sv_asyncSendSnapshot", "0");
                            AppendParameter(ref svParameters, "+sv_scriptCompileAsync", "0");
                            AppendParameter(ref svParameters, "+cl_scriptCompileAsync", "0");
                            AppendParameter(ref svParameters, "+cl_async_bone_setup", "0");
                            AppendParameter(ref svParameters, "+cl_updatedirty_async", "0");
                            AppendParameter(ref svParameters, "+mat_syncGPU", "1");
                            AppendParameter(ref svParameters, "+mat_sync_rt", "1");
                            AppendParameter(ref svParameters, "+mat_sync_rt_flushes_gpu", "1");
                            AppendParameter(ref svParameters, "+net_async_sendto", "0");
                            AppendParameter(ref svParameters, "+physics_async_sv", "0");
                            AppendParameter(ref svParameters, "+physics_async_cl", "0");
                        }

                        AppendHostParameters(ref svParameters);
                        AppendVideoParameters(ref svParameters);

                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Command_Line)))
                            AppendParameter(ref svParameters, (string)Ini.Get(Ini.Vars.Command_Line));

                        return svParameters;
                    }
                case eMode.SERVER:
                    {
                        // GAME ###############################################################
                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Map)))
                            AppendParameter(ref svParameters, "+map", (string)Ini.Get(Ini.Vars.Map));

                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Playlist)))
                            AppendParameter(ref svParameters, "+launchplaylist", (string)Ini.Get(Ini.Vars.Playlist));

                        if ((bool)Ini.Get(Ini.Vars.Enable_Developer))
                        {
                            AppendParameter(ref svParameters, "-dev");
                            AppendParameter(ref svParameters, "-devsdk");
                        }

                        if ((bool)Ini.Get(Ini.Vars.Enable_Cheats))
                        {
                            AppendParameter(ref svParameters, "-dev");
                            AppendParameter(ref svParameters, "-showdevmenu");
                        }

                        // ENGINE ###############################################################
                        if ((bool)Ini.Get(Ini.Vars.No_Async))
                        {
                            AppendParameter(ref svParameters, "-noasync");
                            AppendParameter(ref svParameters, "+async_serialize", "0");
                            AppendParameter(ref svParameters, "+sv_asyncAIInit", "0");
                            AppendParameter(ref svParameters, "+sv_asyncSendSnapshot", "0");
                            AppendParameter(ref svParameters, "+sv_scriptCompileAsync", "0");
                            AppendParameter(ref svParameters, "+physics_async_sv", "0");
                        }

                        AppendHostParameters(ref svParameters);

                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Command_Line)))
                            AppendParameter(ref svParameters, (string)Ini.Get(Ini.Vars.Command_Line));

                        return svParameters;
                    }
                case eMode.CLIENT:
                    {
                        // Tells the loader module to only load the client dll.
                        AppendParameter(ref svParameters, "-noserverdll");

                        // GAME ###############################################################
                        if ((bool)Ini.Get(Ini.Vars.Enable_Developer))
                        {
                            AppendParameter(ref svParameters, "-dev");
                            AppendParameter(ref svParameters, "-devsdk");
                        }

                        if ((bool)Ini.Get(Ini.Vars.Enable_Cheats))
                        {
                            AppendParameter(ref svParameters, "-dev");
                            AppendParameter(ref svParameters, "-showdevmenu");
                        }

                        // ENGINE ###############################################################
                        if ((bool)Ini.Get(Ini.Vars.No_Async))
                        {
                            AppendParameter(ref svParameters, "-noasync");
                            AppendParameter(ref svParameters, "+async_serialize", "0");
                            AppendParameter(ref svParameters, "+buildcubemaps_async", "0");
                            AppendParameter(ref svParameters, "+sv_asyncAIInit", "0");
                            AppendParameter(ref svParameters, "+sv_asyncSendSnapshot", "0");
                            AppendParameter(ref svParameters, "+sv_scriptCompileAsync", "0");
                            AppendParameter(ref svParameters, "+cl_scriptCompileAsync", "0");
                            AppendParameter(ref svParameters, "+cl_async_bone_setup", "0");
                            AppendParameter(ref svParameters, "+cl_updatedirty_async", "0");
                            AppendParameter(ref svParameters, "+mat_syncGPU", "1");
                            AppendParameter(ref svParameters, "+mat_sync_rt", "1");
                            AppendParameter(ref svParameters, "+mat_sync_rt_flushes_gpu", "1");
                            AppendParameter(ref svParameters, "+net_async_sendto", "0");
                            AppendParameter(ref svParameters, "+physics_async_sv", "0");
                            AppendParameter(ref svParameters, "+physics_async_cl", "0");
                        }

                        AppendVideoParameters(ref svParameters);

                        // MAIN ###############################################################
                        if (!string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Command_Line)))
                            AppendParameter(ref svParameters, (string)Ini.Get(Ini.Vars.Command_Line));

                        return svParameters;
                    }
                default:
                    return "";
            }
        }
    }
}