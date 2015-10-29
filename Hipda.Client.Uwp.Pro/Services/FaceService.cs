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
            new FaceItem { FaceFile = "/Assets/Faces/default_smile.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_sweat.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_huffy.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_cry.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_titter.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_handshake.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_victory.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_curse.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_dizzy.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_shutup.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_funk.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_loveliness.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_sad.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_biggrin.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_cool.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_mad.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_shocked.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_tongue.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_lol.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_shy.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/default_sleepy.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_01.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_02.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_03.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_04.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_05.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_06.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_07.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_08.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_09.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_10.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_11.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_12.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_13.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_14.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_15.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/coolmonkey_16.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_01.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_02.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_03.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_04.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_05.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_06.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_07.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_08.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_09.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_10.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_11.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_12.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_13.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_14.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_15.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_16.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_17.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_18.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_19.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_20.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_21.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_22.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_23.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/grapeman_24.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_01.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_02.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_03.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_04.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_05.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_06.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_07.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_08.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_09.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_10.png", FaceValue = ""},
            new FaceItem { FaceFile = "/Assets/Faces/baoman_11.png", FaceValue = ""},
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
        };
    }
}
