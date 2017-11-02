using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TaskSchedule
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlPath = System.Environment.CurrentDirectory+"\\Config.xml";
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlPath);

            string res = "车型：";
            //任务调度算法
            //定义汽车组
            List<string> cars = new List<string>();
            XmlNodeList carList = xml.SelectNodes("Root/Cars/Car");

            cars.Add("大货车");
            cars.Add("小货车");
            cars.Add("轿车");
            cars.Add("三轮车");
            cars.Add("摩托车");
            cars.Add("电动自行车");
            cars.Add("摩拜");
            cars.Add("ofo");
            foreach (string car in cars)
            {
                res += car + ",";
            }
            res = res.TrimEnd(',');

            res += "\n司机列表：";
            //定义一个司机组
            Dictionary<string, string> drivers = new Dictionary<string, string>();
            Dictionary<string, string> backupdrivers = new Dictionary<string, string>();
            drivers.Add("司机1","轿车");
            drivers.Add("司机2", "摩托车");
            drivers.Add("司机3", "自行车");
            drivers.Add("司机4", "小货车");
            drivers.Add("司机5", "轿车");
            drivers.Add("司机6", "大货车");
            drivers.Add("司机7", "ofo");
            foreach (var driver in drivers)
            {
                res += driver.Key + ","+driver.Value+";";
                backupdrivers.Add(driver.Key,driver.Value);
            }
            res = res.TrimEnd(',');
            res += "\n";


            //定义一个任务组
            res += "\n任务列表：";
            Dictionary<string, List<string>> tasks = new Dictionary<string, List<string>>();
            tasks.Add("任务1", new List<string> { "大货车","自行车","ofo" });
            tasks.Add("任务2", new List<string> { "ofo" });
            tasks.Add("任务3", new List<string> { "轿车","摩托车" });
            tasks.Add("任务4", new List<string> { "自行车", "摩托车","ofo","轿车" });
            tasks.Add("任务5", new List<string> { "摩托车"});
            foreach (var task in tasks)
            {
                res += task.Key + "{";
                foreach(var name in task.Value)
                {
                    res += name + ",";
                }
                res = res.TrimEnd(',');
                res += "}\n";
            }
            res = res.TrimEnd(',');
            res += "\n";

            //定义一个任务和司机的对应组
            Dictionary<string, List<string>> taskDriversContainRepeat = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> taskDriversNotContailRepeat = new Dictionary<string, List<string>>();
            
            //保存一个满足所有任务的列表
            foreach (var task in tasks)
            {
                res += task.Key + ":";
                List<string> taskDriver = new List<string>();
                foreach (var driverInTask in task.Value)
                { 
                    if (drivers.ContainsValue(driverInTask))
                    {
                        var driver = drivers.FirstOrDefault(d => d.Value == driverInTask);
                        if (drivers != null)
                        {
                            res += driver.Key+",";
                            taskDriver.Add(driver.Key);
                        }
                    }
                }
                res = res.TrimEnd(',');
                res += "\n";
                taskDriversContainRepeat.Add(task.Key, taskDriver);
            }

            //保存一个没有重复司机的列表
            res += "\n";            
            foreach (var task in tasks)
            {
                res += task.Key + ":";
                List<string> taskDriver = new List<string>();
                foreach (var driverInTask in task.Value)
                {
                    if (drivers.ContainsValue(driverInTask))
                    {
                        var driver = drivers.FirstOrDefault(d => d.Value == driverInTask);
                        if (drivers != null)
                        {
                            res += driver.Key + ",";
                            drivers.Remove(driver.Key);
                            taskDriver.Add(driver.Key);
                        }
                    }
                }
                res = res.TrimEnd(',');
                res += "\n";
                taskDriversNotContailRepeat.Add(task.Key, taskDriver);
            }

            //上面只是第一部分，然后还要对结果进行优化，最低的要求，每个任务至少要安排一个司机
            res += "\n";
            foreach (var taskDriver in taskDriversNotContailRepeat)
            {
                //只处理没有分配司机的任务
                if (taskDriver.Value == null || taskDriver.Value.Count == 0)
                {
                    var driversContainRepeat = taskDriversContainRepeat[taskDriver.Key];
                    bool isbreak = false;
                    foreach (string driver in driversContainRepeat)
                    {
                        if (isbreak)
                        {
                            break;
                        }
                        //根据这个司机的对应车型找到符合条件的多个司机，然后遍历这多个司机来匹配
                        string carName = backupdrivers[driver];
                        List<string> tempDrives = new List<string>();
                        foreach (var tempDriver in backupdrivers)
                        {
                            if (tempDriver.Value == carName)
                            {
                                tempDrives.Add(tempDriver.Key);
                            }
                        }
                        //查询一下这个司机被分配到哪个任务里了，那个任务如果不是只有当前这个司机，就把这个司机重新指定到这里
                        foreach (var taskDriverContainRepeat in taskDriversContainRepeat)
                        {
                            if (taskDriver.Key == taskDriverContainRepeat.Key)
                            {
                                continue;
                            }
                            List<string> taskdrivers = taskDriverContainRepeat.Value;
                            List<string> notContailRepeat = taskDriversNotContailRepeat[taskDriverContainRepeat.Key];
                            if (notContailRepeat != null && notContailRepeat.Count > 1)
                            {
                                foreach (var tempDriverName in tempDrives)
                                {
                                    if (taskdrivers.Contains(tempDriverName))
                                    {
                                        //匹配到司机，然后退出循环
                                        res += taskDriverContainRepeat.Key + "的" + tempDriverName + "移动到" + taskDriver.Key + "\n";
                                        taskDriver.Value.Add(tempDriverName);
                                        taskDriversNotContailRepeat[taskDriverContainRepeat.Key].Remove(tempDriverName);
                                        isbreak = true;
                                        break;
                                    }   
                                }                                
                            }
                        }
                    }
                }
            }
            res += "\n";
            foreach (var taskDriver in taskDriversNotContailRepeat)
            {
                res += taskDriver.Key + ":";
                foreach (string driver in taskDriver.Value)
                {
                    res += driver + ",";
                }
                res = res.TrimEnd(',');
                res += "\n";
            }            
            Console.WriteLine(res);
            Console.ReadLine();
        }
    }
}
