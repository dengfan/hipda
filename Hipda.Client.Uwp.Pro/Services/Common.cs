using System;
using System.Collections.Generic;
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
