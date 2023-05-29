using Newtonsoft.Json.Serialization;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GPTManager : MonoBehaviour
{
    public static GPTManager Instance;
    private void Awake()
    {
        Instance = this; 
    }


    /// <summary>
    /// 存储所有的对话
    /// </summary>
    List<ChatMessage> allChatMessage = new List<ChatMessage>();
    public void ClearAllChatMessage(){ allChatMessage.Clear(); }

    /// <summary>
    /// 当前一次对话的所有结果
    /// </summary>
    List<string> allChatResult = new List<string>();
    public void ClearAllChatResult() { allChatResult.Clear(); }

    OpenAIApi openAIApi = null;
    string gamStyle = "欧洲中世纪RPG游戏";
    public GamePanel gamePanel = null;
    public StartPanel startPanel = null;

    private void Start()
    {
        openAIApi = new OpenAIApi("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");

        Create_Image();
    }

    public void StartGPT()
    {
        ClearAllChatResult();
        ClearAllChatMessage();

        //新建一个openai
        //老秦的
        openAIApi = new OpenAIApi("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");
        //laofei 歇菜了
        //openAIApi = new OpenAIApi("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", "org-OFjYM7WHHrdMmO8lWndXsjDS");

        //openAIApi = new OpenAIApi("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", "org-OFjYM7WHHrdMmO8lWndXsjDS");


        SendRequest();

        
    }
    public async Task Create_Image()
    {
        //var req = new CreateImageRequest
        //{
        //    Prompt = gamStyle+"中的NPC",
        //    N = 1,
        //    Size = "256x256",
        //};
        //var res = await openAIApi.CreateImage(req);
        //req.
        //Debug.Log(res.Data.Count);
        //Debug.Log(res.Data.ToString());
        //OpenAI.ImageData imageData = res.Data[0];
        //Debug.Log(imageData.Url);

        //GetImageByUnityWebRequest(startPanel.myImage, imageData.Url);
        await CreateImage(gamStyle+"中的NPC", startPanel.myImage);

        //Base64ToImag(startPanel.myImage, res.Data[0].B64Json, new Vector2(256, 256));
    }
    public async Task CreateImage(string msg,Image image)
    {
        var req = new CreateImageRequest
        {
            Prompt = msg,
            N = 1,
            Size = "256x256",
        };
        var res = await openAIApi.CreateImage(req);

        OpenAI.ImageData imageData = res.Data[0];
        Debug.Log(imageData.Url);

        GetImageByUnityWebRequest(image, imageData.Url);
    }

    public async void AskChatGPT(string q)
    {
        ChatMessage chatMessage = new ChatMessage()
        {
            Role = "user",
            Content = q.Trim()
        };
        allChatMessage.Add(chatMessage);
        SendRequest();

    }
    private async void SendRequest()
    {
        //如果之前没有聊过天，那么就是给他提示词
        if (allChatMessage.Count==0)
        {
            ChatMessage chat = new ChatMessage()
            {
                Role = "user",
                Content = "你扮演"+ gamStyle+"中的NPC，你每次只会提出一个问题，回答都只给出两个选项，你会根据用户的选择推进题目，每次只能提出一个问题，指引玩家在游戏中探险，允许玩家回复的内容要用\"[\"和\"]\"包裹起来，只有玩家输入内容时你才会继续生成，否则不进行生成，让我们开始这个游戏吧，请先用10个字左右叙述游戏的背景故事，并提出问题，提供给玩家的两个选项要用\"[\"和\"]\"包裹起来，最后一个字符一定是\"]\"。"
                //Content = "你扮演中国古代的RPG游戏中的叙事者，你每次只会提出一个问题，和给出两个答案，答案用\"[\"和\"]\"包裹起来，在用户输入这两个答案其中一个的时候你要继续推进这个游戏，让我们开始这个游戏吧，请先用简单叙述游戏的背景故事，并给出问题。"

            };
            allChatMessage.Add(chat);
        }

        //创建一个聊天
        var req = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            MaxTokens= 1024,
            Messages = allChatMessage,
            Temperature = 0.6f,
            Stream = true
        };

        allChatResult.Clear();

        openAIApi.CreateChatCompletionAsync(req,
            (responses) =>
            {
                var result = string.Join("", responses.Select(response => response.Choices[0].Delta.Content));
                allChatResult.Add(result);
                gamePanel.msgTxt.text=result;
                Debug.Log(result);
            },
            async () =>
            {
                //构造一个系统对话放进聊天记录
                string nowChatResult = allChatResult[allChatResult.Count - 1];
                ChatMessage chatMessage = new ChatMessage()
                {
                    Role = "system",
                    Content = nowChatResult
                };
                allChatMessage.Add(chatMessage);
                //
                Debug.Log("completed:" + nowChatResult);
                if (nowChatResult == ""||nowChatResult==null)
                {
                    Debug.LogError("别玩了，歇菜了，账号免费期限到了（3个月）");
                }

                string input = nowChatResult;
                Regex regex = new Regex(@"\[(.*?)\]"); // 正则表达式用于匹配[]内的任何字符
                MatchCollection matches = regex.Matches(input);

                List<string> messages = new List<string>();
                foreach (Match match in matches)
                {
                    messages.Add(match.Groups[1].Value);
                    Debug.Log("选项有:" + match.Groups[1].Value);
                    //Console.WriteLine(match.Groups[1].Value); // 打印匹配到的文字
                }

                for (int i = 0; i < messages.Count; i++)
                {
                    switch (i)
                    {
                        case 0: { gamePanel.select1Txt.text = messages[i]; } break;
                        case 1: { gamePanel.select2Txt.text = messages[i]; } break;
                        default:
                            break;
                    }
                }

                //在创建图片
                await CreateImage("画一个卡通风格的，"+chatMessage.Content, gamePanel.bgImage);
            },
            new CancellationTokenSource()
        );


    }


    /// <summary>
    /// UnityWebRequest请求
    /// </summary>
    /// <param name="_imageComp">image组件</param>
    /// <param name="_url">URL</param>
    public void GetImageByUnityWebRequest(Image _imageComp, string _url)
    {
        StartCoroutine(UnityWebRequestGetData(_imageComp, _url));
    }
    IEnumerator UnityWebRequestGetData(Image _imageComp, string _url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(_url))
        {
            yield return uwr.SendWebRequest();

            //if (uwr.isHttpError || uwr.isNetworkError) Debug.Log(uwr.error);
            if (uwr.result == UnityWebRequest.Result.ConnectionError) Debug.Log(uwr.error);
            else if (uwr.result == UnityWebRequest.Result.ProtocolError) Debug.Log(uwr.error);
            else if (uwr.result == UnityWebRequest.Result.DataProcessingError) Debug.Log(uwr.error);
            else
            {
                if (uwr.isDone)
                {
                    int width = 256;
                    int height = 256;
                    Texture2D texture2d = new Texture2D(width, height);
                    texture2d = DownloadHandlerTexture.GetContent(uwr);
                    Sprite tempSprite = Sprite.Create(texture2d, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
                    _imageComp.sprite = tempSprite;
                    Resources.UnloadUnusedAssets();
                }
            }
        }
    }

    //base64转图片
    public string Base64ToTexture2d(string Base64STR,Vector2 size)
    {
        Texture2D pic = new Texture2D((int)size.x, (int)size.y);
        byte[] data = System.Convert.FromBase64String(Base64STR);
        pic.LoadImage(data);
        byte[] bytes = pic.EncodeToPNG();

        //下面是为了方便了解图片的信息写的
        string year = System.DateTime.Now.Year.ToString();
        string month = System.DateTime.Now.Month.ToString();
        string day = System.DateTime.Now.Day.ToString();
        string hour = System.DateTime.Now.Hour.ToString();
        string minute = System.DateTime.Now.Minute.ToString();
        string secend = System.DateTime.Now.Second.ToString();
        //存储路径
        string FileFullPath = Application.dataPath/*这是获取assets前的文件路径*/ + "/" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + secend + ".png";
        
        File.WriteAllBytes(FileFullPath, bytes);
        return FileFullPath;
    }
    //图片转base64string
    public string Texture2dToBase64(string texture2d_path)
    {
        //将图片文件转为流文件
        FileStream fs = new System.IO.FileStream(texture2d_path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        byte[] thebytes = new byte[fs.Length];

        fs.Read(thebytes, 0, (int)fs.Length);
        //转为base64string
        string base64_texture2d = Convert.ToBase64String(thebytes);
        Debug.LogError(base64_texture2d);
        //string base64Str = base64_texture2d;
        return base64_texture2d;

    }

    public void Base64ToImag(Image imgComponent, string base64strs,Vector2 size)
    {
        //string base64 = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNzIiIGhlaWdodD0iOTIiPjxwYXRoIGZpbGw9IiNFQTQzMzUiIGQ9Ik0xMTUuNzUgNDcuMThjMCAxMi43Ny05Ljk5IDIyLjE4LTIyLjI1IDIyLjE4cy0yMi4yNS05LjQxLTIyLjI1LTIyLjE4QzcxLjI1IDM0LjMyIDgxLjI0IDI1IDkzLjUgMjVzMjIuMjUgOS4zMiAyMi4yNSAyMi4xOHptLTkuNzQgMGMwLTcuOTgtNS43OS0xMy40NC0xMi41MS0xMy40NFM4MC45OSAzOS4yIDgwLjk5IDQ3LjE4YzAgNy45IDUuNzkgMTMuNDQgMTIuNTEgMTMuNDRzMTIuNTEtNS41NSAxMi41MS0xMy40NHoiLz48cGF0aCBmaWxsPSIjRkJCQzA1IiBkPSJNMTYzLjc1IDQ3LjE4YzAgMTIuNzctOS45OSAyMi4xOC0yMi4yNSAyMi4xOHMtMjIuMjUtOS40MS0yMi4yNS0yMi4xOGMwLTEyLjg1IDkuOTktMjIuMTggMjIuMjUtMjIuMThzMjIuMjUgOS4zMiAyMi4yNSAyMi4xOHptLTkuNzQgMGMwLTcuOTgtNS43OS0xMy40NC0xMi41MS0xMy40NHMtMTIuNTEgNS40Ni0xMi41MSAxMy40NGMwIDcuOSA1Ljc5IDEzLjQ0IDEyLjUxIDEzLjQ0czEyLjUxLTUuNTUgMTIuNTEtMTMuNDR6Ii8+PHBhdGggZmlsbD0iIzQyODVGNCIgZD0iTTIwOS43NSAyNi4zNHYzOS44MmMwIDE2LjM4LTkuNjYgMjMuMDctMjEuMDggMjMuMDctMTAuNzUgMC0xNy4yMi03LjE5LTE5LjY2LTEzLjA3bDguNDgtMy41M2MxLjUxIDMuNjEgNS4yMSA3Ljg3IDExLjE3IDcuODcgNy4zMSAwIDExLjg0LTQuNTEgMTEuODQtMTN2LTMuMTloLS4zNGMtMi4xOCAyLjY5LTYuMzggNS4wNC0xMS42OCA1LjA0LTExLjA5IDAtMjEuMjUtOS42Ni0yMS4yNS0yMi4wOSAwLTEyLjUyIDEwLjE2LTIyLjI2IDIxLjI1LTIyLjI2IDUuMjkgMCA5LjQ5IDIuMzUgMTEuNjggNC45NmguMzR2LTMuNjFoOS4yNXptLTguNTYgMjAuOTJjMC03LjgxLTUuMjEtMTMuNTItMTEuODQtMTMuNTItNi43MiAwLTEyLjM1IDUuNzEtMTIuMzUgMTMuNTIgMCA3LjczIDUuNjMgMTMuMzYgMTIuMzUgMTMuMzYgNi42MyAwIDExLjg0LTUuNjMgMTEuODQtMTMuMzZ6Ii8+PHBhdGggZmlsbD0iIzM0QTg1MyIgZD0iTTIyNSAzdjY1aC05LjVWM2g5LjV6Ii8+PHBhdGggZmlsbD0iI0VBNDMzNSIgZD0iTTI2Mi4wMiA1NC40OGw3LjU2IDUuMDRjLTIuNDQgMy42MS04LjMyIDkuODMtMTguNDggOS44My0xMi42IDAtMjIuMDEtOS43NC0yMi4wMS0yMi4xOCAwLTEzLjE5IDkuNDktMjIuMTggMjAuOTItMjIuMTggMTEuNTEgMCAxNy4xNCA5LjE2IDE4Ljk4IDE0LjExbDEuMDEgMi41Mi0yOS42NSAxMi4yOGMyLjI3IDQuNDUgNS44IDYuNzIgMTAuNzUgNi43MiA0Ljk2IDAgOC40LTIuNDQgMTAuOTItNi4xNHptLTIzLjI3LTcuOThsMTkuODItOC4yM2MtMS4wOS0yLjc3LTQuMzctNC43LTguMjMtNC43LTQuOTUgMC0xMS44NCA0LjM3LTExLjU5IDEyLjkzeiIvPjxwYXRoIGZpbGw9IiM0Mjg1RjQiIGQ9Ik0zNS4yOSA0MS40MVYzMkg2N2MuMzEgMS42NC40NyAzLjU4LjQ3IDUuNjggMCA3LjA2LTEuOTMgMTUuNzktOC4xNSAyMi4wMS02LjA1IDYuMy0xMy43OCA5LjY2LTI0LjAyIDkuNjZDMTYuMzIgNjkuMzUuMzYgNTMuODkuMzYgMzQuOTEuMzYgMTUuOTMgMTYuMzIuNDcgMzUuMy40N2MxMC41IDAgMTcuOTggNC4xMiAyMy42IDkuNDlsLTYuNjQgNi42NGMtNC4wMy0zLjc4LTkuNDktNi43Mi0xNi45Ny02LjcyLTEzLjg2IDAtMjQuNyAxMS4xNy0yNC43IDI1LjAzIDAgMTMuODYgMTAuODQgMjUuMDMgMjQuNyAyNS4wMyA4Ljk5IDAgMTQuMTEtMy42MSAxNy4zOS02Ljg5IDIuNjYtMi42NiA0LjQxLTYuNDYgNS4xLTExLjY1bC0yMi40OS4wMXoiLz48L3N2Zz4=";
        Texture2D tex2D = new Texture2D((int)size.x,(int)size.y);
        try
        {
            byte[] bytes = Convert.FromBase64String(base64strs);
            tex2D.LoadImage(bytes);
            Debug.LogError("转换成功");
        }
        catch (Exception ex)
        {
            Debug.LogError("转换失败" + ex);

        }
        Sprite s = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
        imgComponent.sprite = s;
        Resources.UnloadUnusedAssets();
    }

}
