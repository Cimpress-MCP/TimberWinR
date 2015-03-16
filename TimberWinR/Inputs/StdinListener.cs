﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using TimberWinR.Codecs;
using TimberWinR.Parser;

namespace TimberWinR.Inputs
{
    public class StdinListener : InputListener
    {
        private Thread _listenThread;
        private CodecArguments _codecArguments;
        private ICodec _codec;     
       
        public StdinListener(TimberWinR.Parser.Stdin arguments, CancellationToken cancelToken)
            : base(cancelToken, "Win32-Console")
        {
            _codecArguments = arguments.CodecArguments;
            if (_codecArguments != null && _codecArguments.Type == CodecArguments.CodecType.multiline)
                _codec = new Multiline(_codecArguments);

            _listenThread = new Thread(new ThreadStart(ListenToStdin));
            _listenThread.Start();
        }

        public override JObject ToJson()
        {
            JObject json = new JObject(
                new JProperty("stdin", "enabled"));

            
            if (_codecArguments != null)
            {
                var cp = new JProperty("codec",
                    new JArray(
                        new JObject(
                            new JProperty("type", _codecArguments.Type.ToString()),
                            new JProperty("what", _codecArguments.What.ToString()),
                            new JProperty("negate", _codecArguments.Negate),
                            new JProperty("multilineTag", _codecArguments.MultilineTag),
                            new JProperty("pattern", _codecArguments.Pattern))));
                json.Add(cp);              
            }

            return json;
        }

        public override void Shutdown()
        {
            LogManager.GetCurrentClassLogger().Info("Shutting Down {0}", InputType);
            base.Shutdown();
        }

        private void ListenToStdin()
        {
            LogManager.GetCurrentClassLogger().Info("StdIn Ready");

            while (!CancelToken.IsCancellationRequested)
            {
                string line = Console.ReadLine();
                if (line != null)
                {
                    string msg = ToPrintable(line);

                    if (_codecArguments != null && _codecArguments.Type == CodecArguments.CodecType.multiline)                   
                        _codec.Apply(msg, this);                   
                    else
                    {
                        JObject jo = new JObject();
                        jo["message"] = msg;
                        AddDefaultFields(jo);
                        ProcessJson(jo);
                    }
                }              
            }
            Finished();
        }
    }
}
