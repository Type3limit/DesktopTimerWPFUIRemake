using DesktopTimer.Models.ChatRoom.Defination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.MessageHandler
{
    public class MessageHandlerFactory
    {
        private static readonly Dictionary<string, IMessageHandler> _handlers = new Dictionary<string, IMessageHandler>();

        public static void RegisterHandler(string messageType, IMessageHandler handler)
        {
            if (!_handlers.ContainsKey(messageType))
            {
                _handlers.Add(messageType, handler);
            }
        }

        public static IMessageHandler? GetHandler(string? messageType)
        {
            if (messageType!=null && _handlers.ContainsKey(messageType))
            {
                return _handlers[messageType];
            }
            return null;
        }


        public static void LoadAllHandlers()
        {
            var handlerType = typeof(IMessageHandler);

            var handlerTypes = Assembly.GetExecutingAssembly()
                                       .GetTypes()
                                       .Where(t => handlerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                       .ToList();

            foreach (var type in handlerTypes)
            {
               var curInstance =  (IMessageHandler?)Activator.CreateInstance(type);
                if(curInstance!=null)
                {
                    RegisterHandler(curInstance.AssignedMessageHeader, curInstance);
                }
            }
        }
    }
}
