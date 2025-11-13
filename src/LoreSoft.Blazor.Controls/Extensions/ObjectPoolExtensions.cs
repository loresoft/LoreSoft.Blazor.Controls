using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Extensions;

public static class ObjectPoolExtensions
{
    extension(StringBuilder)
    {
        public static ObjectPool<StringBuilder> Pool
        {
            get
            {
                static void resetAction(StringBuilder sb)
                {
                    sb.Clear();

                    // trim excess capacity if it has grown too large
                    if (sb.Capacity > 1024)
                        sb.Capacity = 256;

                }

                return new(objectFactory: static () => new(256), resetAction: resetAction);
            }
        }
    }
}
