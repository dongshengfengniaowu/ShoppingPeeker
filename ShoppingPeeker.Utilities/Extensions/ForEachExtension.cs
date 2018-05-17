﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class ForEachExtension
    {
        //示范： this.GetControls<Button>(null).ForEach(b => b.Enabled = false);
        /// <summary>
        /// 变量枚举元素  并执行Action
        /// foreach 操作进行的是顺序迭代，不会执行并行任务。
        /// 并行请使用 ForAll<T>；
        /// 参考：https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/introduction-to-plinq
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {

            if (source is ParallelQuery<T>)
            {
                throw new Exception("ParallelQuery Not Support This Extentsion! Please Use ForAll<T>");
            }
            foreach (var item in source)
                action(item);
        }



        /// <summary>
        /// 在使用可迭代类型结合进行Count()的替代
        /// 防止  yield return 带来的性能问题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            if (null==source)
            {
                return true;
            }
            return !source.Any();
        }
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            if (null==source)
            {
                return false;
            }
            return source.Any();
        }

        public static T[] Remove<T>(this T[] objects, Func<T, bool> condition)
        {
            var hopeToDeleteObjs = objects.Where(condition);

            T[] newObjs = new T[objects.Length - hopeToDeleteObjs.Count()];

            int counter = 0;
            for (int i = 0; i < objects.Length; i++)
            {
                if (!hopeToDeleteObjs.Contains(objects[i]))
                {
                    newObjs[counter] = objects[i];
                    counter += 1;
                }
            }

            return newObjs;
        }
    }
}
