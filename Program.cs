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
            TaskScheduleMain();
        }

        /// <summary>
        /// 任务调度的主方法
        /// </summary>
        private static void TaskScheduleMain()
        {
            string xmlPath = System.Environment.CurrentDirectory + "\\Config.xml";
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlPath);

            string res = "车型：";
            //任务调度算法
            //定义汽车组
            List<string> cars = new List<string>();
            XmlNodeList nodeList = xml.SelectNodes("Root/Cars/Car");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    cars.Add(node.InnerText);
                    res += node.InnerText + ",";
                }
            }
            res = res.TrimEnd(',');
            res += "\n司机列表：";
            //定义一个司机组
            Dictionary<string, string> drivers = new Dictionary<string, string>();
            Dictionary<string, string> backupdrivers = new Dictionary<string, string>();
            nodeList = xml.SelectNodes("Root/Drivers/Driver");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    res += node.Attributes["key"].InnerText + "," + node.Attributes["value"].InnerText + ";";
                    drivers.Add(node.Attributes["key"].InnerText, node.Attributes["value"].InnerText);
                    backupdrivers.Add(node.Attributes["key"].InnerText, node.Attributes["value"].InnerText);
                }
            }
            res = res.TrimEnd(',');
            res += "\n";
            //定义一个任务组
            res += "\n任务列表：";
            Dictionary<string, List<string>> tasks = new Dictionary<string, List<string>>();
            nodeList = xml.SelectNodes("Root/Tasks/Task");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    res += node.Attributes["key"].InnerText + "{";
                    List<string> carNames = new List<string>();
                    if (node.HasChildNodes)
                    {
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            res += childNode.InnerText + ",";
                            carNames.Add(childNode.InnerText);
                        }
                    }
                    res = res.TrimEnd(',');
                    res += "}\n";
                    tasks.Add(node.Attributes["key"].InnerText, carNames);
                }
            }
            res = res.TrimEnd(',');
            res += "\n";
            //定义一个任务和司机的对应组
            Dictionary<string, List<string>> taskDriversNotContailRepeat = new Dictionary<string, List<string>>();
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
                    List<string> tempDrives = new List<string>();
                    var tempTaskCars = tasks[taskDriver.Key];
                    foreach (var tempCar in tempTaskCars)
                    {
                        foreach (var tempDriver in backupdrivers)
                        {
                            if (tempDriver.Value == tempCar)
                            {
                                tempDrives.Add(tempDriver.Key);
                            }
                        }
                    }
                    //查询一下这个司机被分配到哪个任务里了，那个任务如果不是只有当前这个司机，就把这个司机重新指定到这里
                    foreach (var taskDriverNotContainRepeat in taskDriversNotContailRepeat)
                    {
                        if (taskDriver.Key == taskDriverNotContainRepeat.Key)
                        {
                            continue;
                        }
                        List<string> taskdrivers = taskDriverNotContainRepeat.Value;
                        List<string> notContailRepeat = taskDriversNotContailRepeat[taskDriverNotContainRepeat.Key];
                        if (notContailRepeat != null && notContailRepeat.Count > 1)
                        {                            
                            foreach (var tempDriverName in tempDrives)
                            {
                                bool isContinue = false;
                                if (taskdrivers.Contains(tempDriverName))
                                {
                                    /*还要加一个循环，检测一下最需要这个司机的任务，如果可以匹配到的话是不是就可以直接
                                    根据循环司机来插入相应的任务中呢？*/
                                    string carName = backupdrivers[tempDriverName];
                                    foreach (var tempTask in tasks)
                                    {
                                        if (tempTask.Key != taskDriver.Key && tempTask.Value.Count == 1 && tempTask.Value[0] == carName)
                                        {
                                            isContinue = true;
                                            break;
                                        }
                                    }
                                    if (isContinue)
                                    {
                                        continue;
                                    }
                                    //匹配到司机，填充，然后退出循环
                                    res += taskDriverNotContainRepeat.Key + "的" + tempDriverName + "移动到" + taskDriver.Key + "\n";
                                    taskDriver.Value.Add(tempDriverName);
                                    taskDriversNotContailRepeat[taskDriverNotContainRepeat.Key].Remove(tempDriverName);
                                    break;
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

        /// <summary>
        /// 尝试根据司机来反向匹配，计算出来每一个司机最应该分配跟哪个任务
        /// </summary>
        private static void TaskScheduleByDriveFilter()
        {
            string xmlPath = System.Environment.CurrentDirectory + "\\Config.xml";
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlPath);

            string res = "车型：";
            //任务调度算法
            //定义汽车组
            List<string> cars = new List<string>();
            XmlNodeList nodeList = xml.SelectNodes("Root/Cars/Car");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    cars.Add(node.InnerText);
                    res += node.InnerText + ",";
                }
            }
            res = res.TrimEnd(',');
            res += "\n司机列表：";
            //定义一个司机组
            Dictionary<string, string> drivers = new Dictionary<string, string>();
            Dictionary<string, string> backupdrivers = new Dictionary<string, string>();
            nodeList = xml.SelectNodes("Root/Drivers/Driver");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    res += node.Attributes["key"].InnerText + "," + node.Attributes["value"].InnerText + ";";
                    drivers.Add(node.Attributes["key"].InnerText, node.Attributes["value"].InnerText);
                    backupdrivers.Add(node.Attributes["key"].InnerText, node.Attributes["value"].InnerText);
                }
            }
            res = res.TrimEnd(',');
            res += "\n";
            //定义一个任务组
            res += "\n任务列表：";
            Dictionary<string, List<string>> tasks = new Dictionary<string, List<string>>();
            nodeList = xml.SelectNodes("Root/Tasks/Task");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    res += node.Attributes["key"].InnerText + "{";
                    List<string> carNames = new List<string>();
                    if (node.HasChildNodes)
                    {
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            res += childNode.InnerText + ",";
                            carNames.Add(childNode.InnerText);
                        }
                    }
                    res = res.TrimEnd(',');
                    res += "}\n";
                    tasks.Add(node.Attributes["key"].InnerText, carNames);
                }
            }
            res = res.TrimEnd(',');
            res += "\n";
            //定义一个任务和司机的对应组
            Dictionary<string, List<string>> taskDriversNotContailRepeat = new Dictionary<string, List<string>>();
            foreach(var task in tasks)
            {
                taskDriversNotContailRepeat.Add(task.Key,new List<string>());
            }

            //遍历司机
            foreach (var driver in drivers)
            {
                string car = driver.Value;
                //判断这个车型最适合分配给那个任务
                foreach (var task in tasks)
                {
                    //不包含就继续循环
                    if (!task.Value.Contains(car))
                    {
                        continue;
                    }
                    //如果该任务已经添加过了相应的车型就不再加了
                    if (taskDriversNotContailRepeat[task.Key].Contains(car))
                    {
                        continue;
                    }
                    //包含的情况下需要计算最匹配的任务，匹配有问题的，最短长度满足不了要求


                    //最终匹配到的情况下，添加到相应的任务里
                    taskDriversNotContailRepeat[task.Key].Add(driver.Key);
                }
            }
        }
    }
}
