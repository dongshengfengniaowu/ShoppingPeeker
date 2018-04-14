using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;


using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ShoppingPeeker.Utilities.TypeFinder;
using ShoppingPeeker.Utilities;
using ShoppingPeeker.DbManage.Utilities;
using ShoppingPeeker.DbManage.CommandTree;




namespace ShoppingPeeker.DbManage
{


    /// <summary>
    /// �������ݿ��  ������  ����ִ�������ݿ���н���
    /// </summary>
    public class SqlDbContext<TElement> :BaseSqlOperation<TElement>, IDbContext<TElement>, IDisposable
        where TElement : BaseEntity, new()
    {
        #region Construction and fields




        /// <summary>
        /// ʵ�����������
        /// </summary>
        private string EntityIdentityFiledName = new TElement().GetIdentity().IdentityKeyName;



        /// <summary>
        /// ���������� ���캯��
        /// </summary>
        /// <param name="dbConfig"></param>
        public SqlDbContext(DbConnConfig dbConfig)
        {
            this.DbConfig = dbConfig;
        }



        #endregion


        #region Context methods


        #region  Insert����
        /// <summary>
        /// ���� ʵ��
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Insert(TElement entity)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);

            ///��������������
            var noIdentityPropertys = propertys.Remove(x => x.Name == EntityIdentityFiledName);
            var noIdentityFileds = filelds.Remove(x => x == EntityIdentityFiledName);
            var noIdentityParas = paras.Remove(x => x == string.Format("@{0}", EntityIdentityFiledName));

            var fieldSplitString = String.Join(",", noIdentityFileds);//���ض��ŷָ����ַ��� ���磺ProvinceCode,ProvinceName,Submmary
            var parasSplitString = String.Join(",", noIdentityParas);//����   ���� �Ķ��ŷָ�


            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.Append(string.Format("insert into {0}(", tableInDbName));
            sb_Sql.Append(string.Format("{0})", fieldSplitString));
            sb_Sql.Append(" values (");
            sb_Sql.Append(string.Format("{0})", parasSplitString));
            sb_Sql.Append(";select @@IDENTITY;");


            SqlParameter[] parameters = new SqlParameter[noIdentityParas.Length];
            var settedValueDic = entity.GetSettedValuePropertyDic();
            for (int i = 0; i < noIdentityParas.Length; i++)
            {
                var colName = noIdentityParas[i];
                string key = noIdentityPropertys[i].Name;
                object value = null;//ReflectionHelper.GetPropertyValue(entity, noIdentityPropertys[i]);
                settedValueDic.TryGetValue(key, out value);
                var para = new SqlParameter(colName, value);
                para.IsNullable = true;

                parameters[i] = para;
            }

            //���ӣ����ϴ���  ��������Ĵ���
            //{
            //        new SqlParameter("@ProvinceCode", SqlDbType.NVarChar,15),
            //        new SqlParameter("@ProvinceName", SqlDbType.NVarChar,50),
            //        new SqlParameter("@Submmary", SqlDbType.Text)};
            //parameters[0].Value = model.ProvinceCode;
            //parameters[1].Value = model.ProvinceName;
            //parameters[2].Value = model.Submmary;

            var sqlCmd = sb_Sql.ToString();


            ///������ַ���ƴ�ӹ�����
            sb_Sql.Clear();
            sb_Sql = null;
            var result = this.ExecuteScalar(sqlCmd, parameters);
            if (null != result)
            {
                return int.Parse(result.ToString());
            }
            return Error_Opeation_Result;
        }


        /// <summary>
        /// ����������β�����ʵ��
        /// (ע�⣺sqlbuck���룬��Ч��sqlbuck��ʽ����)
        /// </summary>
        /// <param name="entities"></param>
        public bool InsertMulitiEntities(IEnumerable<TElement> entities)
        {
            var result = -1;


            var count_entities = entities.Count();
            if (count_entities <= 0)
            {
                return false;
            }


            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entities.First(), out tableInDbName, out propertys, out filelds, out paras);

            try
            {

                ///��������������
                var noIdentityPropertys = propertys.Remove(x => x.Name == EntityIdentityFiledName);


                using (var bulk = new SqlBulkCopy(this.DbConfig.ConnString, SqlBulkCopyOptions.UseInternalTransaction))
                {
                    bulk.BulkCopyTimeout = 60;//���ʱʱ��
                    //bulk.BatchSize = 1000;
                    //ָ��д���Ŀ���
                    bulk.DestinationTableName = tableInDbName;
                    //����Դ�е�������Ŀ�������Ե�ӳ���ϵ
                    //bulk.ColumnMappings.Add("ip", "ip");
                    //bulk.ColumnMappings.Add("port", "port");
                    //bulk.ColumnMappings.Add("proto_name", "proto_name");
                    //bulk.ColumnMappings.Add("strategy_id", "strategy_id");
                    //init mapping
                    foreach (var pi in noIdentityPropertys)
                    {
                        bulk.ColumnMappings.Add(pi.Name, pi.Name);
                    }

                    DataTable dt = SqlDataTableExtensions.ConvertListToDataTable<TElement>(entities, ref noIdentityPropertys);//����Դ����

                    DbDataReader reader = dt.CreateDataReader();
                    bulk.WriteToServer(dt);
                }


                result = 1;

            }
            catch (Exception ex)
            {

                //�׳�Native �쳣��Ϣ
                throw ex;
            }


            var isSuccess = result > 0 ? true : false;


            return isSuccess;


        }

        #endregion


        #region Update ���²���
        /// <summary>
        /// ���µ���ģ��
        /// �����»���Ϊ��ģ���������õ�ֵ���ֶλᱻ���µ���������ֵ �����£�
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Update(TElement entity)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return -1;
                throw new Exception("δָ���������������ֶΣ�");
            }

            StringBuilder sb_FiledParaPairs = new StringBuilder("");

            //---����ȫ���£�����Ϊ�趨ֵ�ĸ��£���ֹ�����ֶ�---------
            //for (int i = 0; i < filelds.Length; i++)
            //{
            //    if (filelds[i] != EntityIdentityFiledName)
            //    {
            //        sb_FiledParaPairs.AppendFormat("{0}=@{0},", filelds[i]);
            //    }
            //}
            var settedValueDic = entity.GetSettedValuePropertyDic();

            foreach (var item in settedValueDic)
            {
                var keyProperty = item.Key;
                //var value = item.Value;
                if (keyProperty != EntityIdentityFiledName)
                {
                    sb_FiledParaPairs.AppendFormat("{0}=@{0},", keyProperty);
                }
            }

            //�Ƴ����һ������
            var str_FiledParaPairs = sb_FiledParaPairs.ToString();
            str_FiledParaPairs = str_FiledParaPairs.Remove(str_FiledParaPairs.Length - 1);

            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.Append(string.Format("update {0} set ", tableInDbName));//Set Table
            sb_Sql.Append(str_FiledParaPairs);//������

            //sb_Sql.Append("ProvinceCode=@ProvinceCode,");
            //sb_Sql.Append("ProvinceName=@ProvinceName,");
            //sb_Sql.Append("Submmary=@Submmary");

            sb_Sql.AppendFormat(" where {0}=@{0}", EntityIdentityFiledName);//����

            //�趨����ֵ--------(�ֶ�һһӳ��)
            SqlParameter[] parameters = new SqlParameter[settedValueDic.Count];
            int counter = 0;
            foreach (var item in settedValueDic)
            {
                var keyProperty = item.Key;
                var value = item.Value;
                var paraName = string.Format("@{0}", keyProperty);
                var Parameter = new SqlParameter(paraName, value);
                Parameter.IsNullable = true;
                parameters[counter] = Parameter;


                counter++;
            }

            #region ��������


            //for (int i = 0; i < paras.Length; i++)
            //{
            //    var Parameter = new SqlParameter(paras[i], DbTypeAndCLRType.ConvertClrTypeToDbType(propertys[i].GetType()));
            //    Parameter.Value = propertys[i].GetValue(entity, null);
            //    Parameter.IsNullable = true;
            //    parameters[i] = Parameter;
            //}

            //SqlParameter[] parameters = {
            //        new SqlParameter("@ProvinceCode", SqlDbType.NVarChar,15),
            //        new SqlParameter("@ProvinceName", SqlDbType.NVarChar,50),
            //        new SqlParameter("@Submmary", SqlDbType.Text),
            //        new SqlParameter("@ID", SqlDbType.Int,4)};
            //parameters[0].Value = model.ProvinceCode;
            //parameters[1].Value = model.ProvinceName;
            //parameters[2].Value = model.Submmary;
            //parameters[3].Value = model.ID;
            #endregion

            var sqlCmd = sb_Sql.ToString();
            ///������ַ���ƴ�ӹ�����
            sb_FiledParaPairs.Clear();
            sb_FiledParaPairs = null;
            sb_Sql.Clear();
            sb_Sql = null;
            return ExecuteNonQuery(sqlCmd, parameters);
        }

        /// <summary>
        /// ����Ԫ�� ͨ��  ����������
        /// �����»���Ϊ��ģ���������õ�ֵ���ֶλᱻ���µ���������ֵ �����£�
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int UpdateByCondition(TElement entity, Expression<Func<TElement, bool>> predicate)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return -1;
                throw new Exception("δָ���������������ֶΣ�");
            }


            StringBuilder sb_FiledParaPairs = new StringBuilder("");
            ///����Ҫ���µ���
            var settedValueDic = entity.GetSettedValuePropertyDic();

            foreach (var item in settedValueDic)
            {
                var keyProperty = item.Key;
                //var value = item.Value;
                if (keyProperty != EntityIdentityFiledName)
                {
                    sb_FiledParaPairs.AppendFormat("{0}=@{0},", keyProperty);
                }
            }
            //�Ƴ����һ������
            var str_FiledParaPairs = sb_FiledParaPairs.ToString();
            str_FiledParaPairs = str_FiledParaPairs.Remove(str_FiledParaPairs.Length - 1);

            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.Append(string.Format("update {0} set ", tableInDbName));//Set Table
            sb_Sql.Append(str_FiledParaPairs);//������



            if (null != predicate)
            {
                string where = ResolveLambdaTreeToCondition.ConvertLambdaToCondition<TElement>(predicate);
                sb_Sql.Append(" where ");//��������
                sb_Sql.Append(where);//�����д��в���=ֵ��  ƴ���ַ���
            }




            SqlParameter[] parameters = new SqlParameter[settedValueDic.Count];
            int counter = 0;
            foreach (var item in settedValueDic)
            {
                var keyProperty = item.Key;
                var value = item.Value;
                var paraName = string.Format("@{0}", keyProperty);
                var Parameter = new SqlParameter(paraName, value);
                Parameter.IsNullable = true;
                parameters[counter] = Parameter;


                counter++;
            }


            var sqlCmd = sb_Sql.ToString();

            ///�����ַ�������
            sb_FiledParaPairs.Clear();
            sb_FiledParaPairs = null;
            sb_Sql.Clear();
            sb_Sql = null;


            return ExecuteNonQuery(sqlCmd, parameters);
        }

        #endregion


        #region Select   ��ѯ����

        /// <summary>
        /// ͨ��������ȡ����Ԫ��
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public TElement GetElementById(long id)
        {

            TElement entity = new TElement();

            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return null;
                throw new Exception("δָ���������������ֶΣ�");
            }
            var fieldSplitString = String.Join(",", filelds);//���ض��ŷָ����ַ��� ���磺ProvinceCode,ProvinceName,Submmary

            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.AppendFormat("select {0} ", fieldSplitString);
            sb_Sql.AppendFormat(" from {0} ", tableInDbName);//WITH (NOLOCK) ���ڲ�������ִ�е�������-�����������
            sb_Sql.AppendFormat(" where {0}=@{0};", EntityIdentityFiledName);
            SqlParameter[] parameters = {
                        new SqlParameter()
            };
            parameters[0].ParameterName = string.Format("@{0)", EntityIdentityFiledName);
            parameters[0].Value = id;

            var sqlCmd = sb_Sql.ToString();

            sb_Sql.Clear();
            sb_Sql = null;
            System.Data.Common.DbDataReader reader = null;
            try
            {
                reader = ExecuteReader(sqlCmd, parameters);
                reader.Read();
                entity = reader.ConvertDataReaderToEntity<TElement>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //�ͷŶ�ȡ��
                if (null != reader)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }


            return entity;
        }

        /// <summary>
        /// ͨ���ض���������ѯ��Ԫ�ؼ���
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<TElement> GetElementsByCondition(Expression<Func<TElement, bool>> predicate)
        {
            TElement entity = new TElement();

            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return null;
                throw new Exception("δָ���������������ֶΣ�");
            }
            //��ȡ�ֶ�
            var fieldSplitString = String.Join(",", filelds);//���ض��ŷָ����ַ��� ���磺ProvinceCode,ProvinceName,Submmary
            //������ѯ����
            string whereStr = "1=1";
            if (null != predicate)
            {
                whereStr = ResolveLambdaTreeToCondition.ConvertLambdaToCondition<TElement>(predicate);
            }



            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.AppendFormat("select  {0} ", fieldSplitString);
            sb_Sql.AppendFormat(" from {0} ", tableInDbName);
            sb_Sql.AppendFormat(" where {0};", whereStr);


            var sqlCmd = sb_Sql.ToString();

            sb_Sql.Clear();
            sb_Sql = null;

            System.Data.Common.DbDataReader reader = null;
            //����DataReader  ��ȡ���ݼ���
            List<TElement> dataLst = new List<TElement>();
            try
            {
                reader = ExecuteReader(sqlCmd, null);
                if (null == reader)
                {
                    return null;
                }




                while (reader.Read())
                {
                    var model = reader.ConvertDataReaderToEntity<TElement>();
                    dataLst.Add(model);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                if (null != reader)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }


            return dataLst;
        }



        /// <summary>
        /// ��ҳ��ȡԪ�ؼ���
        /// </summary>
        /// <param name="pageIndex">ҳ����</param>
        /// <param name="pageSize">ҳ��С</param>
        /// <param name="totalRecords">�ܼ�¼��</param>
        /// <param name="totalPages">��ҳ��</param>
        /// <param name="predicate">����</param>
        /// <param name="sortField">�����ֶΣ������ָ�������ֶ� ��ôĬ�ϰ���id ����</param>
        /// <param name="rule">�������</param>
        /// <returns></returns>
        public List<TElement> GetElementsByPagerAndCondition(int pageIndex, int pageSize, out int totalRecords, out int totalPages, Expression<Func<TElement, bool>> predicate, string sortField = null, OrderRule rule = OrderRule.ASC)
        {
            TElement entity = new TElement();

            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                totalRecords = -1;
                totalPages = -1;
                //�������� û�������ֶ�
                return null;
                throw new Exception("δָ���������������ֶΣ�");
            }

            //��ȡ�ֶ�
            var fieldSplitString = String.Join(",", filelds);//���ض��ŷָ����ַ��� ���磺ProvinceCode,ProvinceName,Submmary
            //������ѯ����
            var whereStr = "1=1";
            if (null != predicate)
            {
                whereStr = ResolveLambdaTreeToCondition.ConvertLambdaToCondition<TElement>(predicate);
            }



            StringBuilder sb_Sql = new StringBuilder("EXEC ");
            sb_Sql.Append(Contanst.PageSql_Call_Name);

            SqlParameter[] parameters = {
                    new SqlParameter("@PageIndex",pageIndex),//ҳ����
                    new SqlParameter("@PageSize", pageSize),//ҳ��С
                    new SqlParameter("@TableName", tableInDbName),//������
                    new SqlParameter("@SelectFields", fieldSplitString),//��ѯ���ֶ�
                     new SqlParameter("@ConditionWhere", whereStr), //��ѯ����      
                    new SqlParameter("@SortField", sortField?? EntityIdentityFiledName),//�����ֶ�
                    new SqlParameter("@IsDesc",rule == OrderRule.ASC ? 0 : 1),//������ ������
                    new SqlParameter("@TotalRecords",0),//�ܼ�¼������ѡ������
                    new SqlParameter("@TotalPageCount",0)//��ҳ�������������
                                        };
            //parameters[0].Value = pageIndex;
            //parameters[1].Value = pageSize;
            //parameters[2].Value = tableInDbName;
            //parameters[3].Value = fieldSplitString;
            //parameters[4].Value = whereStr;
            //parameters[5].Value = sortField?? EntityIdentityFiledName;
            //parameters[6].Value = rule == OrderRule.ASC ? 0 : 1;
            parameters[7].Direction = ParameterDirection.Output;
            parameters[8].Direction = ParameterDirection.Output;
            var sql = sb_Sql.ToString();
            //�����ַ�������
            sb_Sql.Clear();
            sb_Sql = null;

            try
            {
                var lst_PagedData = ExecuteStoredProcedureList(sql, parameters);
                //��ѯ��Ϻ� ����������� �����ܼ�¼�� ��ҳ��
                totalRecords = Convert.ToInt32(parameters[7].Value);
                totalPages = Convert.ToInt32(parameters[8].Value);

                return lst_PagedData;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }



        #endregion


        #region Delete ɾ������

        /// <summary>
        /// ɾ��һ��ʵ��
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public int Delete(TElement entity)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return -1;
                throw new Exception("δָ���������������ֶΣ�");
            }



            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.AppendFormat("delete from {0} ", tableInDbName);
            sb_Sql.Append(" where Id=@Id");
            SqlParameter[] parameters = {
                    new SqlParameter("@Id", entity.GetIdentityValue())
            };

            var sqlCmd = sb_Sql.ToString();
            //��������
            sb_Sql.Clear();
            sb_Sql = null;

            try
            {
                return Convert.ToInt32(ExecuteNonQuery(sqlCmd, parameters));

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// ɾ������������ʵ��
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int DeleteByCondition(Expression<Func<TElement, bool>> predicate)
        {
            TElement entity = new TElement();


            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return -1;
                throw new Exception("δָ���������������ֶΣ�");
            }

            //������ѯ����
            var whereStr = "1=1";
            if (null != predicate)
            {

                whereStr = ResolveLambdaTreeToCondition.ConvertLambdaToCondition<TElement>(predicate);

            }


            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.AppendFormat("delete from {0} ", tableInDbName);
            if (null != predicate)
            {
                sb_Sql.AppendFormat("where  {0}  ", whereStr);
            }



            var sqlCmd = sb_Sql.ToString();
            try
            {
                return Convert.ToInt32(ExecuteNonQuery(sqlCmd, null));

            }
            catch (Exception ex)
            {

                throw ex;
            }


        }



        #endregion




        #region Disposable


        //�Ƿ�������
        bool _disposed;
        public void Dispose()
        {

            Dispose(true);
            // This class has no unmanaged resources but it is possible that somebody could add some in a subclass.
            GC.SuppressFinalize(this);

        }
        //����Ĳ�����ʾʾ�Ƿ���Ҫ�ͷ���Щʵ��IDisposable�ӿڵ��йܶ���
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return; //����Ѿ������գ����ж�ִ��
            if (disposing)
            {
                //TODO:�ͷ���Щʵ��IDisposable�ӿڵ��йܶ���

            }
            //TODO:�ͷŷ��й���Դ�����ö���Ϊnull
            _disposed = true;
        }


        #endregion


        #endregion

    }
}
