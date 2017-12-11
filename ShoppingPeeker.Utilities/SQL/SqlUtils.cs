#region Usings

using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace ShoppingPeeker.Utilities.SQL
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The SqlUtils class provides Shared/Static methods for working with SQL Server related code
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public static class SqlUtils
    {

        /// <summary>
        /// ����Ƿ���SqlΣ���ַ�
        /// </summary>
        /// <param name="strInput">Ҫ�ж��ַ���</param>
        /// <returns>�жϽ��</returns>
        public static bool IsSafeSqlString(string strInput)
        {
            //���˵� tab  �س� ���з�
            strInput = strInput.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Replace(" ", "");

            string SqlStr = "1=1|select *|and'|or'|insert into|delete from|alter table|update|create table|create view|drop view|creat eindex|drop index|create procedure|drop procedure|create trigger|drop trigger|create schema|drop schema|create domain|alter domain|drop domain|);|select@|declare@|print@";
            string[] anySqlStr = SqlStr.Split('|');
            foreach (string ss in anySqlStr)
            {
                if (strInput.IndexOf(ss) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static string ToSafeString(this string strInput)
        {
            return strInput.ToSafeString("");
        }

        /// <summary>
        /// ת���ɰ�ȫ�ַ���
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="defaultString"></param>
        /// <returns></returns>
        public static string ToSafeString(this string strInput, string defaultString)
        {
            if (strInput == null)
            {
                return "";
            }

            if (!IsSafeSqlString(strInput))
            {
                return FilterString(strInput, defaultString);
            }
            return strInput;
        }

        public static string FilterString(string strName, string defaultString)
        {
            return strName.Replace("[", defaultString).Replace("]", defaultString).Replace("-", defaultString).Replace("\\", defaultString).Replace(";", defaultString).Replace("//", defaultString).Replace(",", defaultString)
                .Replace("(", defaultString).Replace(")", defaultString).Replace("}", defaultString).Replace("{", defaultString).Replace("%", defaultString).Replace("@", defaultString).Replace("*", defaultString)
                .Replace("!", defaultString).Replace("'", defaultString).Replace("|", defaultString);
        }


    }
}

