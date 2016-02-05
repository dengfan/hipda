using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public static class Common
    {
        public static List<FaceItemModel> FaceData = new List<FaceItemModel>
        {
            new FaceItemModel { Image = "/Assets/Faces/default_smile.png", Text = ":)"},
            new FaceItemModel { Image = "/Assets/Faces/default_sweat.png", Text = ":sweat:"},
            new FaceItemModel { Image = "/Assets/Faces/default_huffy.png", Text = ":huffy:"},
            new FaceItemModel { Image = "/Assets/Faces/default_cry.png", Text = ":cry:"},
            new FaceItemModel { Image = "/Assets/Faces/default_titter.png", Text = ":titter:"},
            new FaceItemModel { Image = "/Assets/Faces/default_handshake.png", Text = ":handshake:"},
            new FaceItemModel { Image = "/Assets/Faces/default_victory.png", Text = ":victory:"},
            new FaceItemModel { Image = "/Assets/Faces/default_curse.png", Text = ":curse:"},
            new FaceItemModel { Image = "/Assets/Faces/default_dizzy.png", Text = ":dizzy:"},
            new FaceItemModel { Image = "/Assets/Faces/default_shutup.png", Text = ":shutup:"},
            new FaceItemModel { Image = "/Assets/Faces/default_funk.png", Text = ":funk:"},
            new FaceItemModel { Image = "/Assets/Faces/default_loveliness.png", Text = ":loveliness:"},
            new FaceItemModel { Image = "/Assets/Faces/default_sad.png", Text = ":("},
            new FaceItemModel { Image = "/Assets/Faces/default_biggrin.png", Text = ":D"},
            new FaceItemModel { Image = "/Assets/Faces/default_cool.png", Text = ":cool:"},
            new FaceItemModel { Image = "/Assets/Faces/default_mad.png", Text = ":mad:"},
            new FaceItemModel { Image = "/Assets/Faces/default_shocked.png", Text = ":o"},
            new FaceItemModel { Image = "/Assets/Faces/default_tongue.png", Text = ":P"},
            new FaceItemModel { Image = "/Assets/Faces/default_lol.png", Text = ":lol:"},
            new FaceItemModel { Image = "/Assets/Faces/default_shy.png", Text = ":shy:"},
            new FaceItemModel { Image = "/Assets/Faces/default_sleepy.png", Value = ":sleepy:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_01.png", Value = "{:2_41:}", Text = ":酷猴1:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_02.png", Value = "{:2_42:}", Text = ":酷猴2:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_03.png", Value = "{:2_43:}", Text = ":酷猴3:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_04.png", Value = "{:2_44:}", Text = ":酷猴4:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_05.png", Value = "{:2_45:}", Text = ":酷猴5:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_06.png", Value = "{:2_46:}", Text = ":酷猴6:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_07.png", Value = "{:2_47:}", Text = ":酷猴7:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_08.png", Value = "{:2_48:}", Text = ":酷猴8:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_09.png", Value = "{:2_49:}", Text = ":酷猴9:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_10.png", Value = "{:2_50:}", Text = ":酷猴10:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_11.png", Value = "{:2_51:}", Text = ":酷猴11:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_12.png", Value = "{:2_52:}", Text = ":酷猴12:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_13.png", Value = "{:2_53:}", Text = ":酷猴13:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_14.png", Value = "{:2_54:}", Text = ":酷猴14:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_15.png", Value = "{:2_55:}", Text = ":酷猴15:"},
            new FaceItemModel { Image = "/Assets/Faces/coolmonkey_16.png", Value = "{:2_56:}", Text = ":酷猴16:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_01.png", Value = "{:3_57:}", Text = ":呆男1:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_02.png", Value = "{:3_58:}", Text = ":呆男2:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_03.png", Value = "{:3_59:}", Text = ":呆男3:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_04.png", Value = "{:3_60:}", Text = ":呆男4:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_05.png", Value = "{:3_61:}", Text = ":呆男5:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_06.png", Value = "{:3_62:}", Text = ":呆男6:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_07.png", Value = "{:3_63:}", Text = ":呆男7:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_08.png", Value = "{:3_64:}", Text = ":呆男8:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_09.png", Value = "{:3_65:}", Text = ":呆男9:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_10.png", Value = "{:3_66:}", Text = ":呆男10:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_11.png", Value = "{:3_67:}", Text = ":呆男11:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_12.png", Value = "{:3_68:}", Text = ":呆男12:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_13.png", Value = "{:3_69:}", Text = ":呆男13:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_14.png", Value = "{:3_70:}", Text = ":呆男14:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_15.png", Value = "{:3_71:}", Text = ":呆男15:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_16.png", Value = "{:3_72:}", Text = ":呆男16:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_17.png", Value = "{:3_73:}", Text = ":呆男17:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_18.png", Value = "{:3_74:}", Text = ":呆男18:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_19.png", Value = "{:3_75:}", Text = ":呆男19:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_20.png", Value = "{:3_76:}", Text = ":呆男20:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_21.png", Value = "{:3_77:}", Text = ":呆男21:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_22.png", Value = "{:3_78:}", Text = ":呆男22:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_23.png", Value = "{:3_79:}", Text = ":呆男23:"},
            new FaceItemModel { Image = "/Assets/Faces/grapeman_24.png", Value = "{:3_80:}", Text = ":呆男24:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_01.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522171e4cb3f0eb26c192.png[/img]", Text = ":暴漫1:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_02.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/140715221714976bc7d3962b5d.png[/img]", Text = ":暴漫2:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_03.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522178f275b517a688ada.png[/img]", Text = ":暴漫3:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_04.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522170e1d99081ff30ca5.png[/img]", Text = ":暴漫4:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_05.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217a3afef9be5dcd55b.png[/img]", Text = ":暴漫5:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_06.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522173e67ecb8c2ec2a46.png[/img]", Text = ":暴漫6:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_07.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217bbbfd48bc66f8f4a.png[/img]", Text = ":暴漫7:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_08.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522173fd492dfd142a3d8.png[/img]", Text = ":暴漫8:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_09.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217dbce21054c18bbbf.png[/img]", Text = ":暴漫9:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_10.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217bb45c300842334b2.png[/img]", Text = ":暴漫10:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_11.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217832ca03e9c8dd244.png[/img]", Text = ":暴漫11:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_12.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217228e48fb9809157b.png[/img]", Text = ":暴漫12:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_13.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217d29ee7ecdccfdf84.png[/img]", Text = ":暴漫13:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_14.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/140715221793d8be3e189f26d1.png[/img]", Text = ":暴漫14:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_15.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217e4db9fd2eae3c4c2.png[/img]", Text = ":暴漫15:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_16.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522177803b63696b5a22d.png[/img]", Text = ":暴漫16:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_17.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217715e829390952912.png[/img]", Text = ":暴漫17:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_18.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522170cab30ad86198afe.png[/img]", Text = ":暴漫18:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_19.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522172c6e90bdbd2df03e.png[/img]", Text = ":暴漫19:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_20.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522173ed44c087d951af1.png[/img]", Text = ":暴漫20:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_21.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522171757582bc9aefe54.png[/img]", Text = ":暴漫21:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_22.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217a1934c5fda807e15.png[/img]", Text = ":暴漫22:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_23.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522172c2407de0b4feaac.png[/img]", Text = ":暴漫23:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_24.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/140715221783463d4d07bdb661.png[/img]", Text = ":暴漫24:"},
            new FaceItemModel { Image = "/Assets/Faces/baoman_25.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522172bd4de9ae043edab.png[/img]", Text = ":暴漫25:"}
        };

        public static string ReplaceFaceLabel(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }

            var mc = new Regex(@":(酷猴|呆男|暴漫)[0-9]{1,2}:").Matches(txt);
            if (mc != null && mc.Count > 0)
            {
                for (int i = 0; i < mc.Count; i++)
                {
                    var m = mc[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    var face = FaceData.FirstOrDefault(f => f.Text == placeHolderLabel);
                    if (face != null && face.Value != null)
                    {
                        txt = txt.Replace(placeHolderLabel, face.Value);
                    }
                }
            }

            return txt;
        }

        public static Dictionary<string, string> EmojiDic = new Dictionary<string, string>
        {
            { ":grinning:",     "\uD83D\uDE00" },
            { ":grin:",         "\uD83D\uDE01" },
            { ":joy:",          "\uD83D\uDE02" },
            { ":smiley:",       "\uD83D\uDE03" },
            { ":smile:",        "\uD83D\uDE04" },
            { ":sweat_smile:",  "\uD83D\uDE05" },
            { ":laughing:",     "\uD83D\uDE06" },
            { ":innocent:",     "\uD83D\uDE07" },
            { ":smiling_imp:",  "\uD83D\uDE08" },
            { ":wink:",         "\uD83D\uDE09" },
            { ":blush:",        "\uD83D\uDE0A" },
            { ":yum:",          "\uD83D\uDE0B" },
            { ":satisfied:",    "\uD83D\uDE0C" },
            { ":heart_eyes:",   "\uD83D\uDE0D" },
            { ":sunglasses:",   "\uD83D\uDE0E" },
            { ":smirk:",        "\uD83D\uDE0F" },

            { ":neutral_face:",                 "\uD83D\uDE10" },
            { ":expressionless:",               "\uD83D\uDE11" },
            { ":unamused:",                     "\uD83D\uDE12" },
            { ":sweat:",                        "\uD83D\uDE13" },
            { ":pensive:",                      "\uD83D\uDE14" },
            { ":confused:",                     "\uD83D\uDE15" },
            { ":confounded:",                   "\uD83D\uDE16" },
            { ":kissing:",                      "\uD83D\uDE17" },
            { ":kissing_heart:",                "\uD83D\uDE18" },
            { ":kissing_smiling_eyes:",         "\uD83D\uDE19" },
            { ":kissing_closed_eyes:",          "\uD83D\uDE1A" },
            { ":stuck_out_tongue:",             "\uD83D\uDE1B" },
            { ":wink2:",                        "\uD83D\uDE1C" },
            { ":stuck_out_tongue_closed_eyes:", "\uD83D\uDE1D" },
            { ":disappointed:",                 "\uD83D\uDE1E" },
            { ":worried:",                      "\uD83D\uDE1F" },

            { ":angry:",            "\uD83D\uDE20" },
            { ":rage:",             "\uD83D\uDE21" },
            { ":cry:",              "\uD83D\uDE22" },
            { ":persevere:",        "\uD83D\uDE23" },
            { ":triumph:",          "\uD83D\uDE24" },
            { ":relieved:",         "\uD83D\uDE25" },
            { ":frowning:",         "\uD83D\uDE26" },
            { ":anguished:",        "\uD83D\uDE27" },
            { ":fearful:",          "\uD83D\uDE28" },
            { ":weary:",            "\uD83D\uDE29" },
            { ":sleepy:",           "\uD83D\uDE2A" },
            { ":tired_face:",       "\uD83D\uDE2B" },
            { ":grimacing:",        "\uD83D\uDE2C" },
            { ":sob:",              "\uD83D\uDE2D" },
            { ":open_mouth:",       "\uD83D\uDE2E" },
            { ":hushed:",           "\uD83D\uDE2F" },

            { ":cold_sweat:",       "\uD83D\uDE30" },
            { ":scream:",           "\uD83D\uDE31" },
            { ":astonished:",       "\uD83D\uDE32" },
            { ":flushed:",          "\uD83D\uDE33" },
            { ":sleeping:",         "\uD83D\uDE34" },
            { ":dizzy_face:",       "\uD83D\uDE35" },
            { ":no_mouth:",         "\uD83D\uDE36" },
            { ":mask:",             "\uD83D\uDE37" },
            { ":smile_cat:",        "\uD83D\uDE38" },
            { ":joy_cat:",          "\uD83D\uDE39" },
            { ":smiley_cat:",       "\uD83D\uDE3A" },
            { ":heart_eyes_cat:",   "\uD83D\uDE3B" },
            { ":smirk_cat:",        "\uD83D\uDE3C" },
            { ":kissing_cat:",      "\uD83D\uDE3D" },
            { ":pouting_cat:",      "\uD83D\uDE3E" },
            { ":crying_cat_face:",  "\uD83D\uDE3F" },

            { ":scream_cat:",               "\uD83D\uDE40" },
            //{ "",                           "\uD83D\uDE41" },
            //{ "",                           "\uD83D\uDE42" },
            //{ "",                           "\uD83D\uDE43" },
            //{ "",                           "\uD83D\uDE44" },
            { ":no_good:",                  "\uD83D\uDE45" },
            { ":man_ok:",                   "\uD83D\uDE46" },
            { ":bow:",                      "\uD83D\uDE47" },
            { ":see_no_evil:",              "\uD83D\uDE48" },
            { ":hear_no_evil:",             "\uD83D\uDE49" },
            { ":speak_no_evil:",            "\uD83D\uDE4A" },
            { ":raised_hand:",              "\uD83D\uDE4B" },
            { ":raised_hands:",             "\uD83D\uDE4C" },
            { ":person_frowning:",          "\uD83D\uDE4D" },
            { ":person_with_pouting_face:", "\uD83D\uDE4E" },
            { ":pray:",                     "\uD83D\uDE4F" },

            //{ "",                     "\uD83D\uDC40" },
            //{ "",                     "\uD83D\uDC41" },
            //{ "",                     "\uD83D\uDC42" },
            //{ "",                     "\uD83D\uDC43" },
            //{ "",                     "\uD83D\uDC44" },
            //{ "",                     "\uD83D\uDC45" },
            //{ "",                     "\uD83D\uDC46" },
            //{ "",                     "\uD83D\uDC47" },
            //{ "",                     "\uD83D\uDC48" },
            //{ "",                     "\uD83D\uDC49" },
            { ":punch:",        "\uD83D\uDC4A" },
            { ":wave:",         "\uD83D\uDC4B" },
            { ":hand_ok:",      "\uD83D\uDC4C" },
            { ":thumbsup:",     "\uD83D\uDC4D" },
            { ":thumbsdown:",   "\uD83D\uDC4E" },
            { ":clap:",         "\uD83D\uDC4F" },

            { ":pear:",         "\uD83C\uDF50" },
            { ":peach:",         "\uD83C\uDF51" },
            { ":cherries:",         "\uD83C\uDF52" },
            { ":strawberry:",         "\uD83C\uDF53" },
            { ":hamburger:",         "\uD83C\uDF54" },
            { ":pizza:",         "\uD83C\uDF55" },
            { ":meat_on_bone:",         "\uD83C\uDF56" },
            { ":poultry_leg:",         "\uD83C\uDF57" },
            { ":rice_cracker:",         "\uD83C\uDF58" },
            { ":rice_ball:",         "\uD83C\uDF59" },
            { ":rice:",         "\uD83C\uDF5A" },
            { ":curry:",         "\uD83C\uDF5B" },
            { ":ramen:",         "\uD83C\uDF5C" },
            { ":spaghetti:",         "\uD83C\uDF5D" },
            { ":bread:",         "\uD83C\uDF5E" },
            { ":fries:",         "\uD83C\uDF5F" },

            { ":ribbon:",         "\uD83C\uDF80" },
            { ":gift:",         "\uD83C\uDF81" },
            { ":birthday:",         "\uD83C\uDF82" },
            { ":jack_o_lantern:",         "\uD83C\uDF83" },
            { ":christmas_tree:",         "\uD83C\uDF84" },
            { ":santa:",         "\uD83C\uDF85" },
            { ":fireworks:",         "\uD83C\uDF86" },
            { ":sparkler:",         "\uD83C\uDF87" },
            { ":balloon:",         "\uD83C\uDF88" },
            { ":tada:",         "\uD83C\uDF89" },
            { ":confetti_ball:",         "\uD83C\uDF8A" },
            { ":tanabata_tree:",         "\uD83C\uDF8B" },
            { ":crossed_flags:",         "\uD83C\uDF8C" },
            { ":bamboo:",         "\uD83C\uDF8D" },
            { ":dolls:",         "\uD83C\uDF8E" },
            { ":flags:",         "\uD83C\uDF8F" },

            { ":skull:",         "\uD83D\uDC80" },
            { ":information_desk_person:",         "\uD83D\uDC81" },
            { ":guardsman:",         "\uD83D\uDC82" },
            { ":dancer:",         "\uD83D\uDC83" },
            { ":lipstick:",         "\uD83D\uDC84" },
            { ":nail_care:",         "\uD83D\uDC85" },
            { ":massage:",         "\uD83D\uDC86" },
            { ":haircut:",         "\uD83D\uDC87" },
            { ":barber:",         "\uD83D\uDC88" },
            { ":syringe:",         "\uD83D\uDC89" },
            { ":pill:",         "\uD83D\uDC8A" },
            { ":kiss:",         "\uD83D\uDC8B" },
            { ":love_letter:",         "\uD83D\uDC8C" },
            { ":ring:",         "\uD83D\uDC8D" },
            { ":gem:",         "\uD83D\uDC8E" },
            { ":couplekiss:",         "\uD83D\uDC8F" },

            { ":bouquet:",         "\uD83D\uDC90" },
            { ":couple_with_heart:",         "\uD83D\uDC91" },
            { ":wedding:",         "\uD83D\uDC92" },
            { ":heartbeat:",         "\uD83D\uDC93" },
            { ":broken_heart:",         "\uD83D\uDC94" },
            { ":two_hearts:",         "\uD83D\uDC95" },
            { ":sparkling_heart:",         "\uD83D\uDC96" },
            { ":heartpulse:",         "\uD83D\uDC97" },
            { ":cupid:",         "\uD83D\uDC98" },
            { ":blue_heart:",         "\uD83D\uDC99" },
            { ":green_heart:",         "\uD83D\uDC9A" },
            { ":yellow_heart:",         "\uD83D\uDC9B" },
            { ":purple_heart:",         "\uD83D\uDC9C" },
            { ":gift_heart:",         "\uD83D\uDC9D" },
            { ":revolving_hearts:",         "\uD83D\uDC9E" },
            { ":heart_decoration:",         "\uD83D\uDC9F" },

            { ":diamond_shape_with_a_dot_inside:",         "\uD83D\uDCA0" },
            { ":bulb:",         "\uD83D\uDCA1" },
            { ":anger:",         "\uD83D\uDCA2" },
            { ":bomb:",         "\uD83D\uDCA3" },
            { ":zzz:",         "\uD83D\uDCA4" },
            { ":boom:",         "\uD83D\uDCA5" },
            { ":sweat_drops:",         "\uD83D\uDCA6" },
            { ":droplet:",         "\uD83D\uDCA7" },
            { ":dash:",         "\uD83D\uDCA8" },
            { ":shit:",         "\uD83D\uDCA9" },
            { ":muscle:",         "\uD83D\uDCAA" },
            { ":dizzy:",         "\uD83D\uDCAB" },
            { ":speech_balloon:",         "\uD83D\uDCAC" },
            { ":thought_balloon:",         "\uD83D\uDCAD" },
            { ":white_flower:",         "\uD83D\uDCAE" },
            //{ "",         "\uD83D\uDCAF" },
        };

        public static string ReplaceEmojiLabel(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }

            var matchs = new Regex(@":[a-z_]{3,30}:").Matches(txt);
            if (matchs != null && matchs.Count > 0)
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    var m = matchs[i];

                    string emojiLabel = m.Groups[0].Value; // 要被替换的标签
                    if (EmojiDic.ContainsKey(emojiLabel))
                    {
                        txt = txt.Replace(emojiLabel, EmojiDic[emojiLabel]);
                    }
                }
            }

            return txt;
        }

        /// <summary>
        /// 过滤掉不能出现在XML中的字符
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        public static Uri GetMiddleAvatarUriByUserId(int userId)
        {
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = userId % 10;
                userId = (userId - s[i]) / 10;
            }

            return new Uri("http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg");
        }

        public static Uri GetBigAvatarUriByUserId(int userId)
        {
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = userId % 10;
                userId = (userId - s[i]) / 10;
            }

            return new Uri("http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_big.jpg");
        }

        public static Uri GetSmallAvatarUriByUserId(int userId)
        {
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = userId % 10;
                userId = (userId - s[i]) / 10;
            }

            return new Uri("http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_small.jpg");
        }

        public static void SendToast(string toastXml)
        {
            toastXml = ReplaceHexadecimalSymbols(toastXml);
            toastXml = ReplaceEmojiLabel(toastXml);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(toastXml);

            // 创建通知实例
            var notification = new ToastNotification(xmlDoc);

            // 显示通知
            var tn = ToastNotificationManager.CreateToastNotifier();
            tn.Show(notification);
        }

        public static async void PostErrorEmailToDeveloper(string errorTitle, string errorDetails)
        {
            string uriStr = @"mailto:appxking@outlook.com?subject=【{0}】发送异常详情给开发者，以帮助开发者更好的解决问题&body={1}";
            uriStr = string.Format(uriStr, errorTitle, errorDetails);
            
            Uri uri = new Uri(uriStr, UriKind.Absolute);
            await Launcher.LaunchUriAsync(uri);
        }

        public static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }
    }
}
