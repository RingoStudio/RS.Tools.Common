using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public class ExportReport
    {
        public static string CompareReport(dynamic older, long olderTime, dynamic newer, long newerTime)
        {
            // data format:
            // <string>ID : {<string>Key:<string>Value, ... }

            // 前提：假定两者的表头是一样的，否则是按照新数据的表头进行对比的
            // 格式：
            // [olderTimeStr] 与 [newerTimeStr] 版本对比
            // 共计 [count] 个项目有变更，共计有[subCount] 个小项有变更
            // [index]. [名称]（[ID]）:
            //      [index].[subIndex] [KEY]: 由 [[olderContent]] 变更为 [[newerContent]]
            //      [index].[subIndex] [KEY]: 增加新内容 [[newerContent]]
            //      [index].[subIndex] [KEY]: 删除原内容 [[olderContent]]
            int titleIndex = 0;
            int index = 0;
            int subIndex = 0;
            int itemCount = 0;
            int subItemCount = 0;

            int updateItemCount = 0;

            var ret = new List<string>
            {
                $"[{TimeHelper.ChinsesTimeDesc(olderTime)}]旧版本与 [{TimeHelper.ChinsesTimeDesc(newerTime)}]新版本的对比报告",
                "",
                "",
                "",
            };
            var subItems = new List<string>();

            // 先对比增删的ID
            List<string> addIDs = new List<string>();
            List<string> deleteIDs = new List<string>();
            List<string> newerIDs = JSONHelper.GetKeys(newer);
            List<string> olderIDs = JSONHelper.GetKeys(older);
            foreach (var id in olderIDs)
            {
                if (!newerIDs.Contains(id)) deleteIDs.Add(id);
            }
            foreach (var id in newerIDs)
            {
                if (!olderIDs.Contains(id)) addIDs.Add(id);
            }

            // 第一部分 删除的项目
            subItems.Clear();
            if (deleteIDs.Count > 0)
            {
                titleIndex++;
                subItems.Add($"{titleIndex}. 在新版本中被移除的项目，共 {deleteIDs.Count} 个");
                index = 0;
                foreach (var id in deleteIDs)
                {
                    var info = older[id];
                    itemCount++;
                    index++;
                    subItems.Add($"  {titleIndex}.{index}. {JSONHelper.ParseString(info["名称"])}({id})");
                    subIndex = 0;
                    foreach (var kv in info)
                    {
                        var key = kv.Name;
                        if (key == "名称" || key == "ID") continue;
                        var val = JSONHelper.ParseString(kv.Value);
                        if (!string.IsNullOrEmpty(val))
                        {
                            subItemCount++;
                            subIndex++;
                            subItems.Add($"    {titleIndex}.{index}.{subIndex}. [{key}]: 删除原内容[{val}]");
                        }
                    }
                }

                ret.AddRange(subItems);
            }

            // 第二部分 新增的项目
            subItems.Clear();
            if (addIDs.Count > 0)
            {
                titleIndex++;
                subItems.Add($"{titleIndex}. 在新版本中新增加的项目，共 {addIDs.Count} 个");
                index = 0;
                foreach (var id in addIDs)
                {
                    var info = newer[id];
                    itemCount++;
                    index++;
                    subItems.Add($"  {titleIndex}.{index}. {JSONHelper.ParseString(info["名称"])}({id})");
                    subIndex = 0;
                    foreach (var kv in info)
                    {
                        var key = kv.Name;
                        if (key == "名称" || key == "ID") continue;
                        var val = JSONHelper.ParseString(kv.Value);
                        if (!string.IsNullOrEmpty(val))
                        {
                            subItemCount++;
                            subIndex++;
                            subItems.Add($"    {titleIndex}.{index}.{subIndex}. [{key}]: 增加新内容[{val}]");
                        }
                    }
                }

                ret.AddRange(subItems);
            }

            // 第三部分，内部更改
            subItems.Clear();
            index = 1;
            titleIndex++;
            foreach (var item in newer)
            {
                var id = item.Name;
                if (addIDs.Contains(id)) continue;
                subIndex = 0;
                var oldContent = older[id] ?? new JObject();
                var newContent = item.Value;
                bool isUpdated = false;
                var subSubItems = new List<string>();
                foreach (var subItem in newContent)
                {
                    var key = subItem.Name;
                    if (key == "ID") continue;
                    var newVal = JSONHelper.ParseString(subItem.Value);
                    var oldVal = JSONHelper.ParseString(oldContent[key]);
                    if (newVal != oldVal)
                    {
                        subIndex++;
                        if (string.IsNullOrEmpty(newVal))
                        {
                            subSubItems.Add($"    {titleIndex}.{index}.{subIndex}. [{key}] 删除原内容[{oldVal}]");
                        }
                        else if (string.IsNullOrEmpty(oldVal))
                        {
                            subSubItems.Add($"    {titleIndex}.{index}.{subIndex}. [{key}] 增加新内容[{newVal}]");
                        }
                        else
                        {
                            subSubItems.Add($"    {titleIndex}.{index}.{subIndex}. [{key}] 由[{oldVal}] 变更为[{newVal}]");
                        }
                    }

                }
                if (subSubItems.Count > 0)
                {
                    var curCount = subSubItems.Count;
                    subSubItems.Insert(0, $"  {titleIndex}.{index}. {JSONHelper.ParseString(newContent["名称"])}({id}) 有 {curCount} 处变动");
                    subItems.AddRange(subSubItems);

                    updateItemCount++;
                    itemCount++;
                    index++;
                }
            }

            if (updateItemCount > 0)
            {
                subItems.Insert(0, $"{titleIndex}. 在新版本有变动的项目，共 {updateItemCount} 个");
            }

            ret.AddRange(subItems);

            ret[1] = $"共计 {itemCount} 个项目有变更，共计 {subItemCount} 处小项有变更{(itemCount > 0 ? "，其中：" : "")}";
            ret[2] = $"{(addIDs.Count > 0 ? $"新增 {addIDs.Count} 个项目，" : "")}{(deleteIDs.Count > 0 ? $"删除 {deleteIDs.Count} 个项目，" : "")}{(updateItemCount > 0 ? $"更新 {updateItemCount} 个项目，" : "")}";

            return string.Join("\n", ret);
        }
    }
}
