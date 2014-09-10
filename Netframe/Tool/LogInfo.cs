using System;
using System.Text;
using System.IO;


namespace Netframe.Tool
{
    /// <summary>

    /// 日志类

    /// </summary>

    public class LogInfo
    {
        /// <summary>

        /// 日志文件大小

        /// </summary>

        private int fileSize;


        /// <summary>

        /// 日志文件的路径

        /// </summary>

        private string fileLogPath;


        /// <summary>

        /// 日志文件的名称

        /// </summary>

        private string logFileName;


        /// <summary>

        /// 构造函数,初始化日志文件大小[2M]

        /// 可以使用相关属性对其进行更改.

        /// </summary>

        public LogInfo()
        {
            //初始化大于2M日志文件将自动删除;

            this.fileSize = 2048 * 1024;//2M

            //默认路径

            this.fileLogPath = @"d:\logFils\";
            this.logFileName = "日志.txt";
        }
        public LogInfo(string filename)
        {
            //初始化大于2M日志文件将自动删除;

            this.fileSize = 2048 * 1024;//2M

            //默认路径

            this.fileLogPath = @"d:\logFils\";
            this.logFileName = filename;
        }
        /// <summary>
        /// 获取或设置定义日志文件大小
        /// </summary>
        public int FileSize
        {
            set
            {
                fileSize = value;
            }
            get
            {
                return fileSize;
            }
        }


        /// <summary>
        /// 获取或设置日志文件的路径
        /// </summary>
        public string FileLogPath
        {
            set
            {
                this.fileLogPath = value;
            }
            get
            {
                return this.fileLogPath;
            }
        }


        /// <summary>
        /// 获取或设置日志文件的名称
        /// </summary>
        public string LogFileName
        {
            set
            {
                this.logFileName = value;
            }
            get
            {
                return this.logFileName;
            }

        }



        /// <summary>
        /// 向指定目录中的指定的文件中追加日志文件
        /// </summary>
        /// <param name="Message">要写入的内容</param>
        public void WriteLog(string Message)
        {
            this.WriteLog(this.logFileName, Message);
        }



        /// <summary>
        /// 向指定目录中的文件中追加日志文件,日志文件的名称将由传递的参数决定.
        /// </summary>
        /// <param name="LogFileName">日志文件的名称,如:mylog.txt ,如果没有自动创建,如果存在将追加写入日志</param>
        /// <param name="Message">要写入的内容</param>
        public void WriteLog(string LogFileName, string Message)
        {

            //DirectoryInfo path=new DirectoryInfo(LogFileName);
            //如果日志文件目录不存在,则创建
            if (!Directory.Exists(this.fileLogPath))
            {
                Directory.CreateDirectory(this.fileLogPath);
            }

            FileInfo finfo = new FileInfo(this.fileLogPath + LogFileName);
            if (finfo.Exists && finfo.Length > fileSize)
            {
                finfo.Delete();
            }
            try
            {
                FileStream fs = new FileStream(this.fileLogPath + LogFileName, FileMode.Append);
                StreamWriter strwriter = new StreamWriter(fs);
                try
                {

                    DateTime d = DateTime.Now;
                    string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                    strwriter.WriteLine("时间:" + time);
                    strwriter.WriteLine(Message);
                    strwriter.WriteLine();
                    strwriter.Flush();
                }
                catch (Exception ee)
                {
                    Console.WriteLine("日志文件写入失败信息:" + ee.ToString());
                }
                finally
                {
                    strwriter.Close();
                    strwriter = null;
                    fs.Close();
                    fs = null;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("日志文件没有打开,详细信息如下");
            }
        }
    }
}
