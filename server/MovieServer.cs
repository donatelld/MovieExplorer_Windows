using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace MovieExplorer
{
    public class MovieServer
    {
        private Socket serverSocket;
        public string ServerPort { get; set; }
        private List<string> IMAGE_FORMAT = new List<string> {"png", "jpg", "gif" };
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;

        public void Start(string port)
        {
            try
            {
                if (StringUtils.isNotBlank(port))
                {
                    this.ServerPort = port;
                }
                else
                {
                    this.ServerPort = "80";
                }
                string ServerIP = GetLocalIP();
                //创建服务端Socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ServerIP), Int32.Parse(ServerPort)));
                serverSocket.Listen(10);
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), serverSocket);
            }
            catch (Exception) { }
        }

        public void Stop()
        {
            if(serverSocket != null) {
                serverSocket.Close();
            }
        }

        public static string GetLocalIP()
        {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (ipa.ToString().StartsWith("192.168"))
                    {
                        return ipa.ToString();
                    }
                }
            }
            return "127.0.0.1";
        }

        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                Socket new_client = socket.EndAccept(ar);  //接收到来自浏览器的代理socket
                //NO.1  并行处理http请求
                socket.BeginAccept(new AsyncCallback(OnAccept), socket); //开始下一次http请求接收   （此行代码放在NO.2处时，就是串行处理http请求，前一次处理过程会阻塞下一次请求处理）

                byte[] recv_buffer = new byte[1024 * 640];
                int real_recv = new_client.Receive(recv_buffer);  //接收浏览器的请求数据
                string recv_request = Encoding.UTF8.GetString(recv_buffer, 0, real_recv);
                //Console.WriteLine(recv_request);  //将请求显示到界面

                Process(recv_request, new_client);  //解析、路由、处理

                //NO.2  串行处理http请求
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 按照HTTP协议格式 解析浏览器发送的请求字符串
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void Process(string request, Socket response)
        {
            //浏览器发送的请求字符串request格式类似这样：
            //GET /index.html HTTP/1.1
            //Host: 127.0.0.1:8081
            //Connection: keep-alive
            //Cache-Control: max-age=0
            //
            //id=123&pass=123       （post方式提交的表单数据，get方式提交数据直接在url中）
            string[] strs = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);  //以“换行”作为切分标志
            if (strs.Length > 0)  //解析出请求路径、post传递的参数(get方式传递参数直接从url中解析)
            {
                string[] items = strs[0].Split(' ');  //items[1]表示请求url中的路径部分（不含主机部分）
                Dictionary<string, string> param = new Dictionary<string, string>();

                if (strs.Contains(""))  //包含空行  说明存在post数据
                {
                    string post_data = strs[strs.Length - 1]; //最后一项
                    if (post_data != "")
                    {
                        string[] post_datas = post_data.Split('&');
                        foreach (string s in post_datas)
                        {
                            param.Add(s.Split('=')[0], s.Split('=')[1]);
                        }
                    }
                }
                Route(items[1], param, response);  //路由处理
            }
        }

        /// <summary>
        /// 按照请求路径（不包括主机部分）  进行路由处理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="param"></param>
        /// <param name="response"></param>
        private void Route(string path, Dictionary<string, string> param, Socket response)
        {
            try
            {
                Console.WriteLine(path);
                if (path.StartsWith("/sendCommand.html")){
                    SendCommand(path, response);
                }
                else
                {
                    Resources(path, response);
                }
            }
            catch (Exception)
            {
                ResponseError(response, "500");
            }
        }

        private void SendCommand(string path, Socket response)
        {
            string statusline = "HTTP/1.1 200 OK\r\n";   //状态行
            byte[] statusline_to_bytes = Encoding.UTF8.GetBytes(statusline);
            string content = processCommand(path);
            byte[] content_to_bytes = Encoding.UTF8.GetBytes(content);
            string header = string.Format("Content-Type:text/html;charset=UTF-8\r\nContent-Length:{0}\r\n", content_to_bytes.Length);
            byte[] header_to_bytes = Encoding.UTF8.GetBytes(header);  //应答头
            response.Send(statusline_to_bytes);  //发送状态行
            response.Send(header_to_bytes);  //发送应答头
            response.Send(new byte[] { (byte)'\r', (byte)'\n' });  //发送空行
            response.Send(content_to_bytes);  //发送正文（html）
            response.Close();
        }

        private string processCommand(string path)
        {
            if (path.IndexOf("key=") < 0) return "0";
            string key = path.Substring(path.IndexOf("key=") + 4);
            if (StringUtils.isBlank(key))
            {
                return "0";
            }
            switch (key)
            {
                case "up":
                    SendKeys.SendWait("{UP}");
                    break;
                case "down":
                    SendKeys.SendWait("{DOWN}");
                    break;
                case "left":
                    SendKeys.SendWait("{LEFT}");
                    break;
                case "right":
                    SendKeys.SendWait("{RIGHT}");
                    break;
                case "ok":
                    SendKeys.SendWait("{ENTER}");
                    break;
                case "return":
                    SendKeys.SendWait("{ESC}");
                    break;
                case "play":
                    keybd_event(Keys.Space, 0, 0, 0);//空格播放、暂停
                    break;
                case "stop":
                    SendKeys.SendWait("%{F4}");
                    break;
                case "backword":
                    SendKeys.SendWait("x");//快退
                    break;
                case "forward":
                    SendKeys.SendWait("c");//快进
                    break;
                case "prev":
                    break;
                case "next":
                    SendKeys.SendWait("{TAB}");
                    break;
                case "subtitle":
                    SendKeys.SendWait("l");//选字幕
                    break;
                case "audio":
                    SendKeys.SendWait("a");//选音频
                    break;
                case "volup":
                    SendKeys.SendWait("{UP}");
                    break;
                case "voldown":
                    SendKeys.SendWait("{DOWN}");
                    break;
                case "mute":
                    //SendKeys.SendWait("m");//静音
                    System.Diagnostics.Process.Start("mmsys.cpl");//切换音频输出
                    break;
                case "show":
                    //SendKeys.SendWait("^0");//显示和隐藏
                    keybd_event(Keys.ControlKey, 0, 0, 0);
                    keybd_event(Keys.D0, 0, 0, 0);
                    keybd_event(Keys.ControlKey, 0, 2, 0);
                    keybd_event(Keys.D0, 0, 2, 0);
                    break;
                case "guide":
                    SendKeys.SendWait("{PGDN}");//切换显示屏幕
                    break;
                case "win1":
                    System.Diagnostics.Process.Start("DisplaySwitch.exe", "/internal");//仅第一屏幕
                    break;
                case "win2":
                    System.Diagnostics.Process.Start("DisplaySwitch.exe", "/external");//仅第二屏幕
                    break;
                case "full"://全屏
                    keybd_event(Keys.ControlKey, 0, 0, 0);
                    keybd_event(Keys.Enter, 0, 0, 0);
                    keybd_event(Keys.ControlKey, 0, 2, 0);
                    keybd_event(Keys.Enter, 0, 2, 0);
                    break;
                case "shutdown":
                    System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");//关机
                    break;
                case "restart":
                    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");//重启
                    break;
                case "zoom":
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);//鼠标单击
                    break;
                case "setup"://切换屏幕投影方式WIN+P
                    keybd_event(Keys.LWin, 0, 0, 0); //按下LWIN
                    keybd_event(Keys.P, 0, 0, 0); //按下P
                    keybd_event(Keys.LWin, 0, 2, 0);  //释放LWIN
                    keybd_event(Keys.P, 0, 2, 0);        //释放P
                    break;
                default:
                    return "0";
            }
            return "1";
        }

        private void Resources(string path, Socket response)
        {
            if(StringUtils.equals(path, "/"))
            {
                path = "/remote.html";
            }
            string format = path.Substring(path.LastIndexOf(".") + 1).ToLower();
            string contentType = "";
            if (IMAGE_FORMAT.Contains(format))
            {
                contentType = "Content-Type:image" + format;
            }else if(StringUtils.equals(format, "html"))
            {
                contentType = "Content-Type:text/html;charset=UTF-8";
            }
            else if (StringUtils.equals(format, "css"))
            {
                contentType = "Content-Type:text/css;charset=UTF-8";
            }
            string statusline = "HTTP/1.1 200 OK\r\n";   //状态行
            byte[] statusline_to_bytes = Encoding.UTF8.GetBytes(statusline);
            if (!File.Exists(path.Substring(1)))
            {
                ResponseError(response, "404");
                return;
            }
            FileStream fs = File.OpenRead(path.Substring(1)); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] content_to_bytes = new Byte[filelength]; //建立一个字节数组 
            fs.Read(content_to_bytes, 0, filelength); //按字节流读取 
            string header = string.Format(contentType + "\r\nContent-Length:{0}\r\n", content_to_bytes.Length);
            byte[] header_to_bytes = Encoding.UTF8.GetBytes(header);  //应答头

            response.Send(statusline_to_bytes);  //发送状态行
            response.Send(header_to_bytes);  //发送应答头
            response.Send(new byte[] { (byte)'\r', (byte)'\n' });  //发送空行
            response.Send(content_to_bytes);  //发送正文（html）

            response.Close();

        }

        private void ResponseError(Socket response, string code)
        {
            byte[] statusline_to_bytes = Encoding.UTF8.GetBytes("HTTP/1.1 "+ code +"\r\n");
            response.Send(statusline_to_bytes);
            response.Close();
        }


    }
}
