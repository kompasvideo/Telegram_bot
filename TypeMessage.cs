using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example_941
{
    enum Type
    {
        Document,
        Photo,
        Audio,
        Voice,
        Video
    }

    class TypeMessage
    {
        public Type type { get; set; }
        public string fileName { get; set; }        
    }
}
