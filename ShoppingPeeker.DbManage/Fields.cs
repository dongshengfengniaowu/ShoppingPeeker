

using System;
using System.Collections.Generic;

using System.Linq.Expressions;


using ShoppingPeeker.DbManage;
using ShoppingPeeker.DbManage.Utilities;

namespace ShoppingPeeker.DbManage
{


    /// <summary>
    /// �ֶ� ���� ������ʹ��Lambda���ʽ �����ֶεİ���
    /// �������ʽ��  Ȼ�����а������ֶη��ý�����������
    /// </summary>
    /// <typeparam name="TStructuralType"> The type to be configured. </typeparam>
    public class Fields<TStructuralType>
        where TStructuralType : BaseEntity
    {





        private  List<string> _Container_Fileds;

        /// <summary>
        /// �Ѿ��������ֶ�����
        /// </summary>
        internal  List<string> Container_Fileds
        {
            get
            {
                if (null == _Container_Fileds)
                {
                    _Container_Fileds = new List<string>();
                }

                return _Container_Fileds;
            }
        }

        public Fields()
        {

        }

        /// <summary>
        /// ����һ���ֶ�����ʵ��
        /// </summary>
        /// <returns></returns>
        public static Fields<TStructuralType> Build()
        {
            return new Fields<TStructuralType>();
        }
        /// <summary>
        ///����ǿ���� int  fload ��struct ��
        /// </summary>
        /// <typeparam name="T"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>

        public  Fields<TStructuralType> WithField<T>(
            Expression<Func<TStructuralType, T>> propertyExpression)
            where T : struct
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����struct��Ϊ�յ���������
        /// </summary>
        /// <typeparam name="T"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField<T>(
            Expression<Func<TStructuralType, T?>> propertyExpression)
            where T : struct
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        ///�����ַ������͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, object>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //var exType = propertyExpression.Body.NodeType;
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        ///�����ַ������͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, string>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //var exType = propertyExpression.Body.NodeType;
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp=(MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// �����ֽ����͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, byte[]>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����decimal���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, decimal>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����decimal?���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, decimal?>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;

        }
        /// <summary>
        ///����DateTime���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>

        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, DateTime>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����DateTime?���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, DateTime?>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����DateTimeOffset���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>

        public Fields<TStructuralType> WithField(
            Expression<Func<TStructuralType, DateTimeOffset>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����DateTimeOffset?���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>

        public Fields<TStructuralType> WithField(
            Expression<Func<TStructuralType, DateTimeOffset?>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����TimeSpan���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, TimeSpan>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

        /// <summary>
        /// ����TimeSpan?���͵��ֶ�
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        public Fields<TStructuralType> WithField(Expression<Func<TStructuralType, TimeSpan?>> propertyExpression)
        {
            // Decompose the expression tree.-----------�������ʽ��
            // ParameterExpression param = (ParameterExpression)propertyExpression.Parameters[0];
            //BinaryExpression operation = (BinaryExpression)propertyExpression.Body;
            //ParameterExpression left = (ParameterExpression)operation.Left;
            //ConstantExpression right = (ConstantExpression)operation.Right;

            var memberExp = (MemberExpression)propertyExpression.Body;
            var filedName = memberExp.Member.Name;
            ///������������ֶ��� ��ô��ӽ�������
            if (!Container_Fileds.Contains(filedName))
            {
                Container_Fileds.Add(filedName);
            }
            return this;
        }

    }
}
