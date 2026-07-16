using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public static class common
    {
        public static List<T> ToListOfObject<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = ToNullableObject<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static T ToNullableObject<T>(this DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        // helped link: https://social.msdn.microsoft.com/Forums/en-US/e175e51b-effc-47b8-844f-7c273d410b0c/reflection-quotobject-of-type-systemdbnull-cannot-be-converted-to-type?forum=csharplanguage
                        if (dr[column.ColumnName] == DBNull.Value)
                            pro.SetValue(obj, null, null);
                        else
                        {
                            try
                            {
                                object value = dr[column.ColumnName];

                                // Get the underlying type for nullable types
                                Type targetType = pro.PropertyType;
                                Type underlyingType = Nullable.GetUnderlyingType(targetType);
                                if (underlyingType != null)
                                {
                                    // It's a nullable type, use the underlying type for conversion
                                    targetType = underlyingType;
                                }

                                // Handle specific type conversions to avoid Double to Single conversion errors
                                if (targetType == typeof(float) && value is double doubleValue)
                                {
                                    pro.SetValue(obj, Convert.ToSingle(doubleValue), null);
                                }
                                else if (targetType == typeof(double) && value is float floatValue)
                                {
                                    pro.SetValue(obj, Convert.ToDouble(floatValue), null);
                                }
                                else if (targetType == typeof(decimal) && (value is double || value is float))
                                {
                                    pro.SetValue(obj, Convert.ToDecimal(value), null);
                                }
                                else if (targetType == typeof(double) && value is decimal decimalValue)
                                {
                                    pro.SetValue(obj, Convert.ToDouble(decimalValue), null);
                                }
                                else
                                {
                                    // Use Convert.ChangeType for other conversions
                                    if (targetType != value.GetType())
                                    {
                                        value = Convert.ChangeType(value, targetType);
                                    }
                                    pro.SetValue(obj, value, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log the conversion error and set to default value
                                System.Diagnostics.Debug.WriteLine($"Type conversion error for property {pro.Name}: {ex.Message}");
                                pro.SetValue(obj, GetDefaultValue(pro.PropertyType), null);
                            }
                        }
                    }
                    else
                        continue;
                }
            }
            return obj;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static T[] ToArrayOfObject<T>(this DataTable dt)
        {
            T[] data = new T[dt.Rows.Count];
            int index = 0;
            foreach (DataRow row in dt.Rows)
            {
                T item = ToNullableObject<T>(row);
                data.SetValue(item, index);
                index++;
            }
            return data;
        }

        public static string image = "0xFFD8FFE000104A46494600010101004800480000FFE1003A4578696600004D4D002A000000080003511000010000000101000000511100040000000100000B13511200040000000100000B1300000000FFDB004300100B0C0E0C0A100E0D0E1211101318281A181616183123251D283A333D3C3933383740485C4E404457453738506D51575F626768673E4D71797064785C656763FFDB0043011112121815182F1A1A2F634238426363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363FFC00011080032003203012200021101031101FFC4001F0000010501010101010100000000000000000102030405060708090A0BFFC400B5100002010303020403050504040000017D01020300041105122131410613516107227114328191A1082342B1C11552D1F02433627282090A161718191A25262728292A3435363738393A434445464748494A535455565758595A636465666768696A737475767778797A838485868788898A92939495969798999AA2A3A4A5A6A7A8A9AAB2B3B4B5B6B7B8B9BAC2C3C4C5C6C7C8C9CAD2D3D4D5D6D7D8D9DAE1E2E3E4E5E6E7E8E9EAF1F2F3F4F5F6F7F8F9FAFFC4001F0100030101010101010101010000000000000102030405060708090A0BFFC400B51100020102040403040705040400010277000102031104052131061241510761711322328108144291A1B1C109233352F0156272D10A162434E125F11718191A262728292A35363738393A434445464748494A535455565758595A636465666768696A737475767778797A82838485868788898A92939495969798999AA2A3A4A5A6A7A8A9AAB2B3B4B5B6B7B8B9BAC2C3C4C5C6C7C8C9CAD2D3D4D5D6D7D8D9DAE2E3E4E5E6E7E8E9EAF2F3F4F5F6F7F8F9FAFFDA000C03010002110311003F00F3FA28A2800ABDA6E9177A992604023538691CE141C67FCE3D455CD2F466631DCDD10B1E5595386DE3AF3EDD38FAF4AEA6C2D628EC65861511892632953D074181E9D2B3751265558BA54D4E5A5CE0AEEDA4B4B9920976EF438CA9C83E845435A3A869DA8C7A8AC57485A7B87C236461CE70307B76E38C71D2AD6A9E1E3A759998DDAC922637C61318CF07073CF27DAAD34CCD493462514514CA2C59D85D5FC852D607948EA4741D7A9E83A1AEA34FF0DDA59A892FC8B89C1CEC53F20E78FAFE3C73D2B33C23A88B3D544127FA9B9C21F66FE13D3DF1F8FB57713D92B7CC9C1AE7AB369D8E1C4559465CBB232860CEE71D318F6AB508CAE07AD34DBF96C4C842FF00334197036C6368F5EE6B1DC8C4D6F6D51CD1333AA0024C3E082148CE083907F3AE53C5D2B3B4249203BBB100F19E3FC4FE75D0D737E2BEB6DF57FF00D96B6A5B9586D25639FA28A2BA0EF0AF44D0BC456B7D69125C4CB1DD0C232B90379F51F5F4EDF967CEE8A89C14D6A6556946AAB33D4F5187204ABDB83542B94D1FC457560CB0DC3BCF667E568C9C951D3E527D31D3A7F3AB1AD6B81808B4EB83B49CB48A0A9C76009E47BFE1EF58AA525A1C8B0D34EC6C4DA958C2EF1C9751ACAA09DBC9E99C8C8E01E3A75AE5356BF7BEBA63B818518F9600C71EBEBCE33CD51A2B68C144EBA7494360A28A2ACD428A28A0028A28A0028A28A0028A28A00FFFD9";


    }
}
