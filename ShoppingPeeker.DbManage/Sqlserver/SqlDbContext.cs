using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using ShoppingPeeker.Utilities.TypeFinder;
using ShoppingPeeker.Utilities;
using ShoppingPeeker.DbManage.Utilities;
using ShoppingPeeker.DbManage.CommandTree;




namespace ShoppingPeeker.DbManage
{


    /// <summary>
    /// �������ݿ��  ������  ����ִ�������ݿ���н���
    /// </summary>
    public class SqlDbContext<TElement> : BaseSqlOperation<TElement>, IDbContext<TElement>, IDisposable
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
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Insert(TElement entity, IDbTransaction transaction = null)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, true, out tableInDbName, out propertys, out filelds, out paras);

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


            var sqlCmd = sb_Sql.ToString();


            ///������ַ���ƴ�ӹ�����
            sb_Sql.Clear();
            sb_Sql = null;

            using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
            {
                var result = conn.ExecuteScalar<int>(sqlCmd, entity, transaction);
                return result;
            }
        }



        /// <summary>
        /// ����������β�����ʵ��
        /// (ע�⣺sqlbuck���룬��Ч��sqlbuck��ʽ����)
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool InsertMulitiEntities(IEnumerable<TElement> entities, IDbTransaction transaction = null)
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
            ResolveEntity(entities.First(), true, out tableInDbName, out propertys, out filelds, out paras);

            try
            {

                ///��������������
                var noIdentityPropertys = propertys.Remove(x => x.Name == EntityIdentityFiledName);

                using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
                {
                    if (conn.State!= ConnectionState.Open)
                    {
                        conn.Open();
                    }

               

                    if (null == transaction)
                    {
                        transaction = conn.BeginTransaction();
                    }
               
                    using (var bulk = new SqlBulkCopy(conn as SqlConnection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
                    {
                        bulk.BulkCopyTimeout = 120;//���ʱʱ��
                        bulk.BatchSize = 1000;
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

                        //DbDataReader reader = dt.CreateDataReader();
                        bulk.WriteToServer(dt);

                        if (null!=transaction)
                        {
                            transaction.Commit();
                        }
                    }

                }

   
                result = 1;

            }
            catch (Exception ex)
            {

                if (null != transaction)
                {
                    transaction.Rollback();
                }
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
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Update(TElement entity, IDbTransaction transaction = null)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, true, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return -1;
                throw new Exception("δָ���������������ֶΣ�");
            }

            StringBuilder sb_FiledParaPairs = new StringBuilder("");


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

            sb_Sql.AppendFormat(" where {0}=@{0}", EntityIdentityFiledName);//����


            var sqlCmd = sb_Sql.ToString();
            ///������ַ���ƴ�ӹ�����
            sb_FiledParaPairs.Clear();
            sb_FiledParaPairs = null;
            sb_Sql.Clear();
            sb_Sql = null;
            using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
            {
                var result = conn.Execute(sqlCmd, entity,transaction);
                return result;
            }
        }

        /// <summary>
        /// ����Ԫ�� ͨ��  ����������
        /// �����»���Ϊ��ģ���������õ�ֵ���ֶλᱻ���µ���������ֵ �����£�
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int UpdateByCondition(TElement entity, Expression<Func<TElement, bool>> predicate, IDbTransaction transaction = null)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, true, out tableInDbName, out propertys, out filelds, out paras);
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


            var sqlCmd = sb_Sql.ToString();

            ///�����ַ�������
            sb_FiledParaPairs.Clear();
            sb_FiledParaPairs = null;
            sb_Sql.Clear();
            sb_Sql = null;


            using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
            {
                var result = conn.Execute(sqlCmd, entity,transaction);
                return result;
            }
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
            ResolveEntity(entity, false, out tableInDbName, out propertys, out filelds, out paras);
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
            sb_Sql.AppendFormat(" where {0}={1};", EntityIdentityFiledName, id);

            var sqlCmd = sb_Sql.ToString();

            sb_Sql.Clear();
            sb_Sql = null;

            try
            {
                using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
                {
                    entity = conn.QueryFirstOrDefault<TElement>(sqlCmd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
            ResolveEntity(entity, false, out tableInDbName, out propertys, out filelds, out paras);
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

            List<TElement> dataLst = null;
            try
            {
                using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
                {
                    dataLst = conn.Query<TElement>(sqlCmd).AsList();
                }

            }
            catch (Exception ex)
            {
                throw ex;
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
            List<TElement> dataLst = new List<TElement>();
            TElement entity = new TElement();

            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, false, out tableInDbName, out propertys, out filelds, out paras);
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


            //���÷�ҳ�洢����
            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.Append(Contanst.PageSql_Call_Name);

            var sqlCmd = sb_Sql.ToString();

            var sqlParas = new DynamicParameters();
            sqlParas.Add("@PageIndex", pageIndex);//ҳ����
            sqlParas.Add("@PageSize", pageSize);//ҳ��С
            sqlParas.Add("@TableName", tableInDbName);//������
            sqlParas.Add("@SelectFields", fieldSplitString);//��ѯ���ֶ�
            //sqlParas.Add("@PrimaryKey", EntityIdentityFiledName);//��ѯ�ı������
            sqlParas.Add("@ConditionWhere", whereStr);//��ѯ����      
            sqlParas.Add("@SortField", sortField ?? EntityIdentityFiledName);//�����ֶ�
            sqlParas.Add("@IsDesc", (int)rule);//������ ������
            sqlParas.Add("@TotalRecords", DbType.Int32, direction: ParameterDirection.Output);//�ܼ�¼������ѡ������
            sqlParas.Add("@TotalPageCount", DbType.Int32, direction: ParameterDirection.Output);//��ҳ�����������


            try
            {
                using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
                {
                    dataLst = conn.Query<TElement>(sqlCmd, sqlParas, commandType: CommandType.StoredProcedure).AsList();
                }

                //��ѯ��Ϻ� ����������� �����ܼ�¼�� ��ҳ��
                totalRecords = sqlParas.Get<int>("@TotalRecords");
                totalPages = sqlParas.Get<int>("@TotalPageCount");

            }
            catch (Exception ex)
            {
                //�׳�Native �쳣��Ϣ
                throw ex;
            }
            finally
            {
                //�����ַ�������
                sb_Sql.Clear();
                sb_Sql = null;
                propertys = null;
                filelds = null;
                paras = null;
            }


            return dataLst;

        }



        #endregion


        #region Delete ɾ������

        /// <summary>
        /// ɾ��һ��ʵ��
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Delete(TElement entity, IDbTransaction transaction = null)
        {
            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, true, out tableInDbName, out propertys, out filelds, out paras);
            if (filelds.Length <= 1)
            {
                //�������� û�������ֶ�
                return -1;
                throw new Exception("δָ���������������ֶΣ�");
            }

            var primaryValue = ReflectionHelper.GetPropertyValue(entity, EntityIdentityFiledName);

            StringBuilder sb_Sql = new StringBuilder();
            sb_Sql.AppendFormat("delete from {0} ", tableInDbName);
            sb_Sql.AppendFormat(" where {0}={1};", EntityIdentityFiledName, primaryValue);


            var sqlCmd = sb_Sql.ToString();

            //��������
            sb_Sql.Clear();
            sb_Sql = null;

            try
            {
                using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
                {
                    var result = conn.Execute(sqlCmd,transaction);
                    return result;
                }

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
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int DeleteByCondition(Expression<Func<TElement, bool>> predicate, IDbTransaction transaction = null)
        {
            TElement entity = new TElement();


            string tableInDbName;
            System.Reflection.PropertyInfo[] propertys;
            string[] filelds;
            string[] paras;
            ResolveEntity(entity, true, out tableInDbName, out propertys, out filelds, out paras);
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
                using (var conn = DatabaseFactory.GetDbConnection(this.DbConfig))
                {
                    var result = conn.Execute(sqlCmd, transaction);
                    return result;
                }

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
