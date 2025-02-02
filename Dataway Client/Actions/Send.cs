﻿using Newtonsoft.Json;
using System;

namespace Dataway_Client.Actions
{
    internal class Send
    {
        /**
         * Gets executed on the user verb 'send'
         */

        public static int Run(Helper.Send opts)
        {
            // spawn pipe client
            var client = PipeSpawner.Spawn(opts.Pipename);

            Console.WriteLine("Connecting to worker process...");
            client.WaitForConnection();

            Console.WriteLine("Sending command...");
            if (!opts.Context)
            {
                var command = new Formats.Send.Command();
                command.File = opts.File;
                command.User = opts.User;
                command.Message = opts.Message;

                // send command as json
                client.PushMessage(JsonConvert.SerializeObject(command));
            }
            else
            {
                var command = new Formats.Send.Context();
                command.File = opts.File;

                // send command as json
                client.PushMessage(JsonConvert.SerializeObject(command));
            }

            // begin action loop
            while (true)
            {
                var resp = client.WaitForResponse();
                var baseData = JsonConvert.DeserializeObject<Formats.Base>(resp);

                // handle Send actions
                if (baseData.Action.ToUpper() == "SEND")
                {
                    // TODO: add send actions
                }

                // handle Generic actions
                if (baseData.Action.ToUpper() == "GENERIC")
                {
                    switch (baseData.Type.ToUpper())
                    {
                        case "MESSAGE":
                            break;

                        case "ERROR":
                            var data = JsonConvert.DeserializeObject<Formats.Generic.Error>(resp);
                            Console.WriteLine("\nThe worker process reported an error.\nError: {0}\nCode: {1}", data.Text, data.Code);
                            return data.Code;

                        case "COMPLETE":
                            return 0;

                        default:
                            break;
                    }
                }
            }
        }
    }
}