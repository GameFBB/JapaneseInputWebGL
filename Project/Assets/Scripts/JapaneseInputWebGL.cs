using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class JapaneseInputWebGL : MonoBehaviour
{
    /// <summary>
    /// WebGL上でローマ字を日本語に変換する（スペースキー）
    /// </summary>

    //インプットフィールド
    [SerializeField] private InputField TextField;
    [SerializeField] private Text Placeholder;  //変換された語句の一時保存先として使用

    //ボタン（変換候補を表示）
    [SerializeField] private GameObject ButtonCandidate1;
    [SerializeField] private GameObject ButtonCandidate2;
    [SerializeField] private GameObject ButtonCandidate3;
    [SerializeField] private GameObject ButtonCandidate4;
    [SerializeField] private GameObject ButtonCandidate5;
    [SerializeField] private GameObject ButtonCandidate6;
    [SerializeField] private GameObject ButtonCandidate7;
    [SerializeField] private Text TextCandidate1;
    [SerializeField] private Text TextCandidate2;
    [SerializeField] private Text TextCandidate3;
    [SerializeField] private Text TextCandidate4;
    [SerializeField] private Text TextCandidate5;
    [SerializeField] private Text TextCandidate6;  //ひらがな
    [SerializeField] private Text TextCandidate7;  //ローマ字

    //変数
    private static string RomanText = "";
    private static List<string> RomanTextList = new List<string>();
    private string AllRomanText;
    private string Hiragana;
    private JsonData JsonObject;
    private int PhraseCount;
    private int PhraseNumber = 0;
    private string[] TextCandidate = new string[7];
    private int NextFocus = 2;
    private static List<string> DeterminedPhraseList = new List<string>();
    private static string AllPhraseDetermined = "";
    private string AddText = "";

    // Update is called once per frame
    void Update()
    {
        //WebGLでのみ動作
        if (Application.platform == RuntimePlatform.WebGLPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor)
        {
            //１．変換キーが押された場合（スペース）
            if (Input.GetKeyUp(KeyCode.Space))
            {
                //入力された空白文字を削除
                TextField.text = TextField.text.Trim();

                //入力されたローマ字を記憶
                RomanText = TextField.text;
                for (int i = 0; i < DeterminedPhraseList.Count; i++)
                {
                    Regex RegexRomanText = new Regex(DeterminedPhraseList[i]);
                    RomanText = RegexRomanText.Replace(RomanText, "", 1);
                }
                RomanTextList.Add(RomanText);

                //変換
                if (TextField.text != "")
                {
                    //ひらがなに変換
                    Placeholder.text = ConvertToHiragana(RomanText);

                    //グーグル日本語入力で変換
                    StartCoroutine(PostGoogleJapaneseInput());
                }
                else
                {
                    Placeholder.text = "Enter text...";
                }
            }

            //２．削除キーが押された場合（バックスペース、デリート）
            if (Input.GetKeyUp(KeyCode.Backspace) || Input.GetKeyUp(KeyCode.Delete))
            {
                //変数のリセット
                TextField.text = "";
                Placeholder.text = "Enter text...";
                RomanText = "";
                RomanTextList.Clear();
                AllRomanText = "";
                PhraseNumber = 0;
                DeterminedPhraseList.Clear();
                AllPhraseDetermined = "";
                AddText = "";
                ButtonCandidate1.SetActive(false);
                ButtonCandidate2.SetActive(false);
                ButtonCandidate3.SetActive(false);
                ButtonCandidate4.SetActive(false);
                ButtonCandidate5.SetActive(false);
                ButtonCandidate6.SetActive(false);
                ButtonCandidate7.SetActive(false);

                //インプットフィールドにフォーカス
                TextField.ActivateInputField();
            }

            //３．TABキーまたは下矢印キーが押された場合
            if (ButtonCandidate1.activeSelf == true && (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.DownArrow)))
            {
                //次の検索候補へフォーカスを移動
                switch (NextFocus)
                {
                    case 1:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate1);
                        break;
                    case 2:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate2);
                        break;
                    case 3:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate3);
                        break;
                    case 4:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate4);
                        break;
                    case 5:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate5);
                        break;
                    case 6:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate6);
                        break;
                    case 7:
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate7);
                        break;
                    default:
                        break;
                }

                switch (NextFocus)
                {
                    case 7:
                        NextFocus = 1;
                        break;
                    default:
                        NextFocus++;
                        break;
                }
            }
        }
    }

    //ひらがな変換
    private string ConvertToHiragana(string RomanLetters)
    {
        //記号
        RomanLetters = RomanLetters.Replace("!", "！");
        RomanLetters = RomanLetters.Replace("#", "＃");
        RomanLetters = RomanLetters.Replace("$", "＄");
        RomanLetters = RomanLetters.Replace("%", "％");
        RomanLetters = RomanLetters.Replace("=", "＝");
        RomanLetters = RomanLetters.Replace("@", "＠");
        RomanLetters = RomanLetters.Replace("+", "＋");
        RomanLetters = RomanLetters.Replace("-", "ー");
        RomanLetters = RomanLetters.Replace("_", "＿");
        RomanLetters = RomanLetters.Replace(",", "、");
        RomanLetters = RomanLetters.Replace(".", "。");
        RomanLetters = RomanLetters.Replace("?", "？");

        //促音・撥音
        RomanLetters = RomanLetters.Replace("kk", "っk");
        RomanLetters = RomanLetters.Replace("ss", "っs");
        RomanLetters = RomanLetters.Replace("tt", "っt");
        RomanLetters = RomanLetters.Replace("nn", "ん");
        RomanLetters = RomanLetters.Replace("hh", "っh");
        RomanLetters = RomanLetters.Replace("mm", "っm");
        RomanLetters = RomanLetters.Replace("yy", "っy");
        RomanLetters = RomanLetters.Replace("rr", "っr");
        RomanLetters = RomanLetters.Replace("gg", "っg");
        RomanLetters = RomanLetters.Replace("zz", "っz");
        RomanLetters = RomanLetters.Replace("dd", "っd");
        RomanLetters = RomanLetters.Replace("bb", "っb");
        RomanLetters = RomanLetters.Replace("pp", "っp");

        //小文字
        RomanLetters = RomanLetters.Replace("xa", "ぁ");
        RomanLetters = RomanLetters.Replace("xi", "ぃ");
        RomanLetters = RomanLetters.Replace("xu", "ぅ");
        RomanLetters = RomanLetters.Replace("xe", "ぇ");
        RomanLetters = RomanLetters.Replace("xo", "ぉ");
        RomanLetters = RomanLetters.Replace("xya", "ゃ");
        RomanLetters = RomanLetters.Replace("xyi", "ぃ");
        RomanLetters = RomanLetters.Replace("xyu", "ゅ");
        RomanLetters = RomanLetters.Replace("xye", "ぇ");
        RomanLetters = RomanLetters.Replace("xyo", "ょ");

        RomanLetters = RomanLetters.Replace("la", "ぁ");
        RomanLetters = RomanLetters.Replace("li", "ぃ");
        RomanLetters = RomanLetters.Replace("lu", "ぅ");
        RomanLetters = RomanLetters.Replace("le", "ぇ");
        RomanLetters = RomanLetters.Replace("lo", "ぉ");
        RomanLetters = RomanLetters.Replace("lya", "ゃ");
        RomanLetters = RomanLetters.Replace("lyi", "ぃ");
        RomanLetters = RomanLetters.Replace("lyu", "ゅ");
        RomanLetters = RomanLetters.Replace("lye", "ぇ");
        RomanLetters = RomanLetters.Replace("lyo", "ょ");

        //ぱ行
        RomanLetters = RomanLetters.Replace("pa", "ぱ");
        RomanLetters = RomanLetters.Replace("pi", "ぴ");
        RomanLetters = RomanLetters.Replace("pu", "ぷ");
        RomanLetters = RomanLetters.Replace("pe", "ぺ");
        RomanLetters = RomanLetters.Replace("po", "ぽ");
        RomanLetters = RomanLetters.Replace("pya", "ぴゃ");
        RomanLetters = RomanLetters.Replace("pyu", "ぴゅ");
        RomanLetters = RomanLetters.Replace("pyo", "ぴょ");

        //ば行
        RomanLetters = RomanLetters.Replace("ba", "ば");
        RomanLetters = RomanLetters.Replace("bi", "び");
        RomanLetters = RomanLetters.Replace("bu", "ぶ");
        RomanLetters = RomanLetters.Replace("be", "べ");
        RomanLetters = RomanLetters.Replace("bo", "ぼ");
        RomanLetters = RomanLetters.Replace("bya", "びゃ");
        RomanLetters = RomanLetters.Replace("byu", "びゅ");
        RomanLetters = RomanLetters.Replace("byo", "びょ");

        //だ行
        RomanLetters = RomanLetters.Replace("da", "だ");
        RomanLetters = RomanLetters.Replace("di", "ぢ");
        RomanLetters = RomanLetters.Replace("du", "づ");
        RomanLetters = RomanLetters.Replace("de", "で");
        RomanLetters = RomanLetters.Replace("do", "ど");
        RomanLetters = RomanLetters.Replace("dya", "ぢゃ");
        RomanLetters = RomanLetters.Replace("dyi", "ぢぃ");
        RomanLetters = RomanLetters.Replace("dyu", "ぢゅ");
        RomanLetters = RomanLetters.Replace("dye", "ぢぇ");
        RomanLetters = RomanLetters.Replace("dyo", "ぢょ");
        RomanLetters = RomanLetters.Replace("dha", "でゃ");
        RomanLetters = RomanLetters.Replace("dhi", "でぃ");
        RomanLetters = RomanLetters.Replace("dhu", "でゅ");
        RomanLetters = RomanLetters.Replace("dhe", "でぇ");
        RomanLetters = RomanLetters.Replace("dho", "でょ");

        //ざ行
        RomanLetters = RomanLetters.Replace("za", "ざ");
        RomanLetters = RomanLetters.Replace("zi", "じ");
        RomanLetters = RomanLetters.Replace("zu", "ず");
        RomanLetters = RomanLetters.Replace("ze", "ぜ");
        RomanLetters = RomanLetters.Replace("zo", "ぞ");
        RomanLetters = RomanLetters.Replace("zya", "じゃ");
        RomanLetters = RomanLetters.Replace("zyi", "じぃ");
        RomanLetters = RomanLetters.Replace("zyu", "じゅ");
        RomanLetters = RomanLetters.Replace("zye", "じぇ");
        RomanLetters = RomanLetters.Replace("zyo", "じょ");

        RomanLetters = RomanLetters.Replace("ja", "じゃ");
        RomanLetters = RomanLetters.Replace("ji", "じ");
        RomanLetters = RomanLetters.Replace("ju", "じゅ");
        RomanLetters = RomanLetters.Replace("je", "じぇ");
        RomanLetters = RomanLetters.Replace("jo", "じょ");
        RomanLetters = RomanLetters.Replace("jya", "じゃ");
        RomanLetters = RomanLetters.Replace("jyi", "じぃ");
        RomanLetters = RomanLetters.Replace("jyu", "じゅ");
        RomanLetters = RomanLetters.Replace("jye", "じぇ");
        RomanLetters = RomanLetters.Replace("jyo", "じょ");

        //が行
        RomanLetters = RomanLetters.Replace("ga", "が");
        RomanLetters = RomanLetters.Replace("gi", "ぎ");
        RomanLetters = RomanLetters.Replace("gu", "ぐ");
        RomanLetters = RomanLetters.Replace("ge", "げ");
        RomanLetters = RomanLetters.Replace("go", "ご");
        RomanLetters = RomanLetters.Replace("gya", "ぎゃ");
        RomanLetters = RomanLetters.Replace("gyi", "ぎぃ");
        RomanLetters = RomanLetters.Replace("gyu", "ぎゅ");
        RomanLetters = RomanLetters.Replace("gye", "ぎぇ");
        RomanLetters = RomanLetters.Replace("gyo", "ぎょ");

        //わ行
        RomanLetters = RomanLetters.Replace("wa", "わ");
        RomanLetters = RomanLetters.Replace("wi", "うぃ");
        RomanLetters = RomanLetters.Replace("wu", "う");
        RomanLetters = RomanLetters.Replace("we", "うぇ");
        RomanLetters = RomanLetters.Replace("wo", "を");
        RomanLetters = RomanLetters.Replace("wyi", "ゐ");
        RomanLetters = RomanLetters.Replace("wye", "ゑ");
        RomanLetters = RomanLetters.Replace("wha", "うぁ");
        RomanLetters = RomanLetters.Replace("whi", "うぃ");
        RomanLetters = RomanLetters.Replace("whu", "う");
        RomanLetters = RomanLetters.Replace("whe", "うぇ");
        RomanLetters = RomanLetters.Replace("who", "うぉ");

        //ら行
        RomanLetters = RomanLetters.Replace("ra", "ら");
        RomanLetters = RomanLetters.Replace("ri", "り");
        RomanLetters = RomanLetters.Replace("ru", "る");
        RomanLetters = RomanLetters.Replace("re", "れ");
        RomanLetters = RomanLetters.Replace("ro", "ろ");
        RomanLetters = RomanLetters.Replace("rya", "りゃ");
        RomanLetters = RomanLetters.Replace("ryi", "りぃ");
        RomanLetters = RomanLetters.Replace("ryu", "りゅ");
        RomanLetters = RomanLetters.Replace("rye", "りぇ");
        RomanLetters = RomanLetters.Replace("ryo", "りょ");

        //ま行
        RomanLetters = RomanLetters.Replace("ma", "ま");
        RomanLetters = RomanLetters.Replace("mi", "み");
        RomanLetters = RomanLetters.Replace("mu", "む");
        RomanLetters = RomanLetters.Replace("me", "め");
        RomanLetters = RomanLetters.Replace("mo", "も");
        RomanLetters = RomanLetters.Replace("mya", "みゃ");
        RomanLetters = RomanLetters.Replace("myi", "みぃ");
        RomanLetters = RomanLetters.Replace("myu", "みゅ");
        RomanLetters = RomanLetters.Replace("mye", "みぇ");
        RomanLetters = RomanLetters.Replace("myo", "みょ");

        //な行
        RomanLetters = RomanLetters.Replace("na", "な");
        RomanLetters = RomanLetters.Replace("ni", "に");
        RomanLetters = RomanLetters.Replace("nu", "ぬ");
        RomanLetters = RomanLetters.Replace("ne", "ね");
        RomanLetters = RomanLetters.Replace("no", "の");
        RomanLetters = RomanLetters.Replace("nya", "にゃ");
        RomanLetters = RomanLetters.Replace("nyi", "にぃ");
        RomanLetters = RomanLetters.Replace("nyu", "にゅ");
        RomanLetters = RomanLetters.Replace("nye", "にぇ");
        RomanLetters = RomanLetters.Replace("nha", "んは");
        RomanLetters = RomanLetters.Replace("nhi", "んひ");
        RomanLetters = RomanLetters.Replace("nhu", "んふ");
        RomanLetters = RomanLetters.Replace("nhe", "へ");
        RomanLetters = RomanLetters.Replace("nho", "んほ");

        //た行
        RomanLetters = RomanLetters.Replace("ta", "た");
        RomanLetters = RomanLetters.Replace("ti", "ち");
        RomanLetters = RomanLetters.Replace("tu", "つ");
        RomanLetters = RomanLetters.Replace("te", "て");
        RomanLetters = RomanLetters.Replace("to", "と");
        RomanLetters = RomanLetters.Replace("tya", "ちゃ");
        RomanLetters = RomanLetters.Replace("tyi", "ちぃ");
        RomanLetters = RomanLetters.Replace("tyu", "ちゅ");
        RomanLetters = RomanLetters.Replace("tye", "ちぇ");
        RomanLetters = RomanLetters.Replace("tyo", "ちょ");
        RomanLetters = RomanLetters.Replace("tha", "てゃ");
        RomanLetters = RomanLetters.Replace("thi", "てぃ");
        RomanLetters = RomanLetters.Replace("thu", "てゅ");
        RomanLetters = RomanLetters.Replace("the", "てぇ");
        RomanLetters = RomanLetters.Replace("tho", "てょ");
        RomanLetters = RomanLetters.Replace("cya", "ちゃ");
        RomanLetters = RomanLetters.Replace("cyi", "ちぃ");
        RomanLetters = RomanLetters.Replace("cyu", "ちゅ");
        RomanLetters = RomanLetters.Replace("cye", "ちぇ");
        RomanLetters = RomanLetters.Replace("cyo", "ちょ");
        RomanLetters = RomanLetters.Replace("cha", "ちゃ");
        RomanLetters = RomanLetters.Replace("chi", "ち");
        RomanLetters = RomanLetters.Replace("chu", "ちゅ");
        RomanLetters = RomanLetters.Replace("che", "ちぇ");
        RomanLetters = RomanLetters.Replace("cho", "ちょ");

        //さ行
        RomanLetters = RomanLetters.Replace("sa", "さ");
        RomanLetters = RomanLetters.Replace("si", "し");
        RomanLetters = RomanLetters.Replace("su", "す");
        RomanLetters = RomanLetters.Replace("se", "せ");
        RomanLetters = RomanLetters.Replace("so", "そ");
        RomanLetters = RomanLetters.Replace("sya", "しゃ");
        RomanLetters = RomanLetters.Replace("syi", "しぃ");
        RomanLetters = RomanLetters.Replace("syu", "しゅ");
        RomanLetters = RomanLetters.Replace("sye", "しぇ");
        RomanLetters = RomanLetters.Replace("syo", "しょ");
        RomanLetters = RomanLetters.Replace("sha", "しゃ");
        RomanLetters = RomanLetters.Replace("shi", "し");
        RomanLetters = RomanLetters.Replace("shu", "しゅ");
        RomanLetters = RomanLetters.Replace("she", "しぇ");
        RomanLetters = RomanLetters.Replace("sho", "しょ");

        //か行
        RomanLetters = RomanLetters.Replace("ka", "か");
        RomanLetters = RomanLetters.Replace("ki", "き");
        RomanLetters = RomanLetters.Replace("ku", "く");
        RomanLetters = RomanLetters.Replace("ke", "け");
        RomanLetters = RomanLetters.Replace("ko", "こ");
        RomanLetters = RomanLetters.Replace("kya", "きゃ");
        RomanLetters = RomanLetters.Replace("kyi", "きぃ");
        RomanLetters = RomanLetters.Replace("kyu", "きゅ");
        RomanLetters = RomanLetters.Replace("kye", "きぇ");
        RomanLetters = RomanLetters.Replace("kyo", "きょ");

        //は行
        RomanLetters = RomanLetters.Replace("ha", "は");
        RomanLetters = RomanLetters.Replace("hi", "ひ");
        RomanLetters = RomanLetters.Replace("hu", "ふ");
        RomanLetters = RomanLetters.Replace("he", "へ");
        RomanLetters = RomanLetters.Replace("ho", "ほ");
        RomanLetters = RomanLetters.Replace("hya", "ひゃ");
        RomanLetters = RomanLetters.Replace("hyi", "ひぃ");
        RomanLetters = RomanLetters.Replace("hyu", "ひゅ");
        RomanLetters = RomanLetters.Replace("hye", "ひぇ");
        RomanLetters = RomanLetters.Replace("hyo", "ひょ");

        RomanLetters = RomanLetters.Replace("fa", "ふぁ");
        RomanLetters = RomanLetters.Replace("fi", "ふぃ");
        RomanLetters = RomanLetters.Replace("fu", "ふ");
        RomanLetters = RomanLetters.Replace("fe", "ふぇ");
        RomanLetters = RomanLetters.Replace("fo", "ふぉ");
        RomanLetters = RomanLetters.Replace("fya", "ふゃ");
        RomanLetters = RomanLetters.Replace("fyu", "ふゅ");
        RomanLetters = RomanLetters.Replace("fyo", "ふょ");

        //や行
        RomanLetters = RomanLetters.Replace("ya", "や");
        RomanLetters = RomanLetters.Replace("yu", "ゆ");
        RomanLetters = RomanLetters.Replace("ye", "いぇ");
        RomanLetters = RomanLetters.Replace("yo", "よ");

        //あ行
        RomanLetters = RomanLetters.Replace("a", "あ");
        RomanLetters = RomanLetters.Replace("i", "い");
        RomanLetters = RomanLetters.Replace("u", "う");
        RomanLetters = RomanLetters.Replace("e", "え");
        RomanLetters = RomanLetters.Replace("o", "お");

        //ん
        RomanLetters = RomanLetters.Replace("n", "ん");

        Hiragana = RomanLetters.Trim();

        //ひらがな出力
        return Hiragana;
    }

    //グーグル日本語入力による変換
    private IEnumerator PostGoogleJapaneseInput()
    {
        string URL = "https://www.google.com/transliterate?langpair=ja-Hira|ja&text=" + Hiragana;

        WWW GoogleJapaneseInput = new WWW(URL);

        yield return GoogleJapaneseInput;

        //デコード
        JsonObject = JsonMapper.ToObject(GoogleJapaneseInput.text);

        //区切られた文節数
        PhraseCount = 0;
        foreach (IList item in JsonObject)
        {
            PhraseCount++;
        }

        //文節1の変換候補を表示
        ButtonCandidate1.SetActive(true);
        ButtonCandidate2.SetActive(true);
        ButtonCandidate3.SetActive(true);
        ButtonCandidate4.SetActive(true);
        ButtonCandidate5.SetActive(true);
        ButtonCandidate6.SetActive(true);
        ButtonCandidate7.SetActive(true);

        TextCandidate[0] = (string)JsonObject[0][0];
        TextCandidate6.text = TextCandidate[0];

        TextCandidate7.text = RomanText;

        TextCandidate[1] = (string)JsonObject[0][1][0];
        TextCandidate1.text = TextCandidate[1];

        try
        {
            TextCandidate[2] = (string)JsonObject[0][1][1];
            TextCandidate[3] = (string)JsonObject[0][1][2];
            TextCandidate[4] = (string)JsonObject[0][1][3];
            TextCandidate[5] = (string)JsonObject[0][1][4];
            TextCandidate2.text = TextCandidate[2];
            TextCandidate3.text = TextCandidate[3];
            TextCandidate4.text = TextCandidate[4];
            TextCandidate5.text = TextCandidate[5];
        }
        catch (System.Exception)
        {
            TextCandidate[2] = (string)JsonObject[0][1][0];
            TextCandidate[3] = (string)JsonObject[0][1][0];
            TextCandidate[4] = (string)JsonObject[0][1][0];
            TextCandidate[5] = (string)JsonObject[0][1][0];
            TextCandidate2.text = "";
            TextCandidate3.text = "";
            TextCandidate4.text = "";
            TextCandidate5.text = "";
        }

        //変換候補１にフォーカス
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate1);
    }

    //変換候補をクリックした時の処理
    public void ClickCandidateButtons(int SelectNumber)
    {
        //文節番号
        PhraseNumber++;

        //Placeholder書き換え
        switch (SelectNumber)
        {
            case 6:
                Placeholder.text = TextCandidate[0];
                break;
            case 7:
                Placeholder.text = RomanText;
                break;
            default:
                Placeholder.text = Placeholder.text.Replace(TextCandidate[0], TextCandidate[SelectNumber]);
                break;
        }

        //次の文節がない場合
        if (PhraseCount == 1 || PhraseNumber >= PhraseCount)
        {
            //決定した変換を記憶
            DeterminedPhraseList.Add((Placeholder.text));

            //変換終了
            PhraseNumber = 0;
            ButtonCandidate1.SetActive(false);
            ButtonCandidate2.SetActive(false);
            ButtonCandidate3.SetActive(false);
            ButtonCandidate4.SetActive(false);
            ButtonCandidate5.SetActive(false);
            ButtonCandidate6.SetActive(false);
            ButtonCandidate7.SetActive(false);

            //ローマ字の文字列
            AllRomanText = "";
            for (int i = 0; i < RomanTextList.Count; i++)
            {
                AllRomanText = AllRomanText + RomanTextList[i];
            }

            //変換後の文字列
            TextField.text = "";
            for (int i = 0; i < DeterminedPhraseList.Count; i++)
            {
                TextField.text = TextField.text + DeterminedPhraseList[i];
            }

            //保存
            AllPhraseDetermined = TextField.text;

            //インプットフィールドにフォーカス
            TextField.ActivateInputField();
        }

        //次の文節がある場合
        else
        {
            //次の文節の変換候補を表示
            TextCandidate[0] = (string)JsonObject[PhraseNumber][0];
            TextCandidate6.text = TextCandidate[0];
            TextCandidate7.text = RomanText;

            TextCandidate[1] = (string)JsonObject[PhraseNumber][1][0];
            TextCandidate1.text = TextCandidate[1];

            try
            {
                TextCandidate[2] = (string)JsonObject[PhraseNumber][1][1];
                TextCandidate[3] = (string)JsonObject[PhraseNumber][1][2];
                TextCandidate[4] = (string)JsonObject[PhraseNumber][1][3];
                TextCandidate[5] = (string)JsonObject[PhraseNumber][1][4];
                TextCandidate2.text = TextCandidate[2];
                TextCandidate3.text = TextCandidate[3];
                TextCandidate4.text = TextCandidate[4];
                TextCandidate5.text = TextCandidate[5];
            }
            catch (System.Exception)
            {
                TextCandidate[2] = (string)JsonObject[PhraseNumber][1][0];
                TextCandidate[3] = (string)JsonObject[PhraseNumber][1][0];
                TextCandidate[4] = (string)JsonObject[PhraseNumber][1][0];
                TextCandidate[5] = (string)JsonObject[PhraseNumber][1][0];
                TextCandidate2.text = "";
                TextCandidate3.text = "";
                TextCandidate4.text = "";
                TextCandidate5.text = "";
            }

            //変換候補1にフォーカス
            NextFocus = 2;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ButtonCandidate1);
        }
    }

    //結果の出力  ※OnEndEditイベントで呼び出し
    public void OnEndEdit()
    {
        //一度も変換されていない場合
        if (AllPhraseDetermined == "")
        {
            AllRomanText = TextField.text;
        }

        //それ以外
        else
        {
            //最後に入力された変換されていないローマ字の追加
            AddText = TextField.text.Replace(AllPhraseDetermined, "");
            AllRomanText = AllRomanText + AddText;
        }

        //出力
        Debug.Log("Before：" + AllRomanText);
        Debug.Log("After：" + TextField.text);
    }
}