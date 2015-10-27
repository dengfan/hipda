using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class FaceItem
    {
        public string FaceFile { get; set; }
        public string FaceValue { get; set; }
    }

    public static class FaceService
    {
        public static List<FaceItem> FaceData = new List<FaceItem>
        {
            new FaceItem { FaceFile = "/Assets/Faces/baoman/1.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/2.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/3.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/4.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/5.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/6.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/7.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/8.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/9.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/10.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/11.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/12.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/13.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/14.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/15.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/16.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/17.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/18.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/19.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/20.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/21.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/22.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/23.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/24.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman/25.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/01.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/02.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/03.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/04.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/05.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/06.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/07.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/08.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/09.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/10.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/11.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/12.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/13.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/14.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/15.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey/16.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/01.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/02.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/03.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/04.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/05.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/06.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/07.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/08.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/09.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/10.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/11.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/12.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/13.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/14.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/15.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/16.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/17.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/18.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/19.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/20.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/21.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/22.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/23.gif", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman/24.gif", FaceValue = ""}
        };
    }
}
