using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xugu.Data.EntityFramework.Utilities;
using XuguClient;

namespace Xugu.Data.EntityFramework
{
	public sealed class XGProviderServices : System.Data.Entity.Core.Common.DbProviderServices
	{
		internal static readonly Xugu.Data.EntityFramework.XGProviderServices Instance = new Xugu.Data.EntityFramework.XGProviderServices();
		private List<string> uniquenKeys = new List<string>();

		static XGProviderServices()
		{
		}

        //protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string manifestToken)
        //{
        //    if (fromReader == null)
        //        throw new ArgumentNullException("fromReader must not be null");

        //    EFXGDataReader efReader = fromReader as EFXGDataReader;
        //    if (efReader == null)
        //    {
        //        throw new ArgumentException(
        //                string.Format(
        //                        "Spatial readers can only be produced from readers of type EFXGDataReader.   A reader of type {0} was provided.",
        //                        fromReader.GetType()));
        //    }

        //    return new XGSpatialDataReader(efReader);
        //}

        protected override DbCommandDefinition CreateDbCommandDefinition(
				DbProviderManifest providerManifest, DbCommandTree commandTree)
		{
			if (commandTree == null)
				throw new ArgumentNullException("commandTree");

			SqlGenerator generator = null;
			if (commandTree is DbQueryCommandTree)
				generator = new SelectGenerator();
			else if (commandTree is DbInsertCommandTree)
				generator = new InsertGenerator();
			else if (commandTree is DbUpdateCommandTree)
				generator = new UpdateGenerator();
			else if (commandTree is DbDeleteCommandTree)
				generator = new DeleteGenerator();
			else if (commandTree is DbFunctionCommandTree)
				generator = new FunctionGenerator();

			string sql = generator.GenerateSQL(commandTree);

			EFXGCommand cmd = new EFXGCommand();
			cmd.CommandText = sql;
			if (generator is FunctionGenerator)
				cmd.CommandType = (generator as FunctionGenerator).CommandType;

			SetExpectedTypes(commandTree, cmd);

			EdmFunction function = null;
			if (commandTree is DbFunctionCommandTree)
				function = (commandTree as DbFunctionCommandTree).EdmFunction;

			// Now make sure we populate the command's parameters from the CQT's parameters:
			foreach (KeyValuePair<string, TypeUsage> queryParameter in commandTree.Parameters)
			{
				DbParameter parameter = cmd.CreateParameter();
				parameter.ParameterName = queryParameter.Key;
				parameter.Direction = ParameterDirection.Input;
				parameter.DbType = Metadata.GetDbType(queryParameter.Value);

				FunctionParameter funcParam;
				if (function != null &&
						function.Parameters.TryGetValue(queryParameter.Key, false, out funcParam))
				{
					parameter.ParameterName = funcParam.Name;
					parameter.Direction = Metadata.ModeToDirection(funcParam.Mode);
					parameter.DbType = Metadata.GetDbType(funcParam.TypeUsage);
				}
				cmd.Parameters.Add(parameter);
			}

			// Now add parameters added as part of SQL gen 
			foreach (DbParameter p in generator.Parameters)
				cmd.Parameters.Add(p);

			return CreateCommandDefinition(cmd);
		}

		/// <summary>
		/// Sets the expected column types
		/// </summary>
		private void SetExpectedTypes(DbCommandTree commandTree, EFXGCommand cmd)
		{
			if (commandTree is DbQueryCommandTree)
				SetQueryExpectedTypes(commandTree as DbQueryCommandTree, cmd);
			else if (commandTree is DbFunctionCommandTree)
				SetFunctionExpectedTypes(commandTree as DbFunctionCommandTree, cmd);
		}

		/// <summary>
		/// Sets the expected column types for a given query command tree
		/// </summary>
		private void SetQueryExpectedTypes(DbQueryCommandTree tree, EFXGCommand cmd)
		{
			DbProjectExpression projectExpression = tree.Query as DbProjectExpression;
			if (projectExpression != null)
			{
				EdmType resultsType = projectExpression.Projection.ResultType.EdmType;

				StructuralType resultsAsStructuralType = resultsType as StructuralType;
				if (resultsAsStructuralType != null)
				{
					cmd.ColumnTypes = new PrimitiveType[resultsAsStructuralType.Members.Count];

					for (int ordinal = 0; ordinal < resultsAsStructuralType.Members.Count; ordinal++)
					{
						EdmMember member = resultsAsStructuralType.Members[ordinal];
						PrimitiveType primitiveType = member.TypeUsage.EdmType as PrimitiveType;
						cmd.ColumnTypes[ordinal] = primitiveType;
					}
				}
			}
		}

		/// <summary>
		/// Sets the expected column types for a given function command tree
		/// </summary>
		private void SetFunctionExpectedTypes(DbFunctionCommandTree tree, EFXGCommand cmd)
		{
			if (tree.ResultType != null)
			{
				Debug.Assert(tree.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType,
			Xugu.Data.EntityFramework.Properties.Resources.WrongFunctionResultType);

				CollectionType collectionType = (CollectionType)(tree.ResultType.EdmType);
				EdmType elementType = collectionType.TypeUsage.EdmType;

				if (elementType.BuiltInTypeKind == BuiltInTypeKind.RowType)
				{
					ReadOnlyMetadataCollection<EdmMember> members = ((RowType)elementType).Members;
					cmd.ColumnTypes = new PrimitiveType[members.Count];

					for (int ordinal = 0; ordinal < members.Count; ordinal++)
					{
						EdmMember member = members[ordinal];
						PrimitiveType primitiveType = (PrimitiveType)member.TypeUsage.EdmType;
						cmd.ColumnTypes[ordinal] = primitiveType;
					}

				}
				else if (elementType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
				{
					cmd.ColumnTypes = new PrimitiveType[1];
					cmd.ColumnTypes[0] = (PrimitiveType)elementType;
				}
				else
				{
					Debug.Fail(Xugu.Data.EntityFramework.Properties.Resources.WrongFunctionResultType);
				}
			}
		}

		protected override string GetDbProviderManifestToken(DbConnection connection)
		{
			// we need the connection option to determine what version of the server
			// we are connected to
			return "12.0.0";
		}

		protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
		{
			return new XGProviderManifest(manifestToken);
		}

		//cxh

		private string GetDBName(string connectionString)
		{
			string pattern = @"DB=([^;]+)";

			Match match = Regex.Match(connectionString, pattern);

			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			else
			{
				throw new Exception("No DB name found in connection string");
			}
		}

		protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");
			XGConnection conn = connection as XGConnection;
			if (conn == null)
				throw new ArgumentException(EntityFramework.Properties.Resources.ConnectionMustBeOfTypeXGConnection, "connection");
				//Ensure a valid provider manifest token.
			string providerManifestToken = this.GetDbProviderManifestToken(connection);
			string query = DbCreateDatabaseScript(providerManifestToken, storeItemCollection);

			using (XGConnection c = new XGConnection())
			{
				string dbName = GetDBName(connection.ConnectionString);
				c.ConnectionString = connection.ConnectionString;
                try
                {
                    c.Open();
                }
                catch(Exception ex)
                {
                    if (ex.Message.Contains("[E34305]"))
                    {
						c.ConnectionString = XGConnectionFactory.GetConfigConnection("XuguClient");//c.ConnectionString.Replace("DB=" + dbName, "DB=" + "SYSTEM");
						c.Open();
						
						
					}
                }

				XGCommand s = new XGCommand();
				s.Connection = c;
				s.CommandText = $"USE SYSTEM;CREATE DATABASE IF NOT EXISTS `{dbName}`; USE `{dbName}`;";
				s.ExecuteNonQuery();
				foreach (EntityContainer container in storeItemCollection.GetItems<EntityContainer>())
				{
					var entitySets = container.BaseEntitySets.OfType<EntitySet>();
					var schemas = new HashSet<string>(entitySets.Select(i => GetSchemaName(i)));
					foreach (var schema in schemas.OrderBy(i => i))
					{
						// don't bother creating default schema
						if (schema != "SYSDBA")
						{
							s.CommandText = $"BEGIN IF (SELECT COUNT(*) FROM ALL_SCHEMAS WHERE SCHEMA_NAME='{schema}')=0 THEN CREATE SCHEMA `{schema}`; END IF; END;";
							s.ExecuteNonQuery();
						}
					}
				}
				string fullQuery = query + "Show Auto_Commit;";
				s.CommandText = fullQuery;
				s.ExecuteNonQuery();
			}
		}

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            XGConnection conn = connection as XGConnection;
            if (conn == null)
                throw new ArgumentException(EntityFramework.Properties.Resources.ConnectionMustBeOfTypeXGConnection, "connection");

            XGConnectionStringBuilder builder = new XGConnectionStringBuilder();
            builder.ConnectionString = conn.ConnectionString;
            string dbName = GetDBName(builder.ConnectionString);

            using (XGConnection c = new XGConnection(builder.ConnectionString))
            {
                try
                {
					c.Open();
					return true;
                }
                catch(Exception ex)
                {
      //              if (ex.Message.Contains("[E34305]"))
      //              {
						//return false;
      //              }
					return false;
                }
            }
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            XGConnection conn = connection as XGConnection;
            if (conn == null)
                throw new ArgumentException(EntityFramework.Properties.Resources.ConnectionMustBeOfTypeXGConnection, "connection");

            XGConnectionStringBuilder builder = new XGConnectionStringBuilder();
            builder.ConnectionString = conn.ConnectionString;
            string dbName = GetDBName(builder.ConnectionString);

            using (XGConnection c = new XGConnection(builder.ConnectionString.Replace("DB=" + dbName, "DB=" + "SYSTEM")))
            {
                c.Open();
                XGCommand cmd = new XGCommand(String.Format("USE SYSTEM;DROP DATABASE IF EXISTS `{0}`;", dbName), c);
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;
                cmd.ExecuteNonQuery();
            }
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken,
				StoreItemCollection storeItemCollection)
		{
			StringBuilder sql = new StringBuilder();

			sql.AppendLine("-- Xugu script");
			sql.AppendLine("-- Created on " + DateTime.Now);

			foreach (EntityContainer container in storeItemCollection.GetItems<EntityContainer>())
			{
				// now output the tables
				foreach (EntitySet es in container.BaseEntitySets.OfType<EntitySet>())
				{
					sql.Append(GetTableCreateScript(es));
				}

				// now output the foreign keys
				foreach (AssociationSet a in container.BaseEntitySets.OfType<AssociationSet>())
				{
					sql.Append(GetAssociationCreateScript(a.ElementType));
				}
			}

			return sql.ToString();
		}

		private string GetAssociationCreateScript(AssociationType a)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder keySql = new StringBuilder();

			if (a.IsForeignKey)
			{
				EntityType childType = (EntityType)a.ReferentialConstraints[0].ToProperties[0].DeclaringType;
				EntityType parentType = (EntityType)a.ReferentialConstraints[0].FromProperties[0].DeclaringType;
				string fkName = a.Name;
				if (fkName.Length > 64)
				{
					fkName = "FK_" + Guid.NewGuid().ToString().Replace("-", "");
				}
				sql.AppendLine(String.Format(
						"ALTER TABLE `{0}` ADD CONSTRAINT {1}", _pluralizedNames[childType.Name], fkName));
				sql.Append("\t FOREIGN KEY (");
				string delimiter = "";
				foreach (EdmProperty p in a.ReferentialConstraints[0].ToProperties)
				{
					EdmMember member;
					if (!childType.KeyMembers.TryGetValue(p.Name, false, out member))
						keySql.AppendLine(String.Format(
								"CREATE INDEX IDX_{0}_{1} on {0}({1});", _pluralizedNames[childType.Name], p.Name));
					sql.AppendFormat("{0}{1}", delimiter, p.Name);
					delimiter = ", ";
				}
				sql.AppendLine(")");
				delimiter = "";
				sql.Append(String.Format("\tREFERENCES `{0}` (", _pluralizedNames[parentType.Name]));
				foreach (EdmProperty p in a.ReferentialConstraints[0].FromProperties)
				{
					EdmMember member;
					if (!parentType.KeyMembers.TryGetValue(p.Name, false, out member))
						keySql.AppendLine(String.Format(
								"CREATE INDEX IDX_{0}_{1} on {0}({1});", _pluralizedNames[parentType.Name], p.Name));
					sql.AppendFormat("{0}{1}", delimiter, p.Name);
					delimiter = ", ";
				}
				sql.AppendLine(")");
				OperationAction oa = a.AssociationEndMembers[0].DeleteBehavior;
				sql.AppendLine(String.Format(" ON DELETE {0} ON UPDATE {1};",
					oa == OperationAction.None ? "NO ACTION" : oa.ToString(), "NO ACTION"));
				sql.AppendLine();
			}

			keySql.Append(sql.ToString());
			return keySql.ToString();
		}

		private Dictionary<string, string> _pluralizedNames = new Dictionary<string, string>();
		private List<string> _guidIdentityColumns;

		internal string GetTableCreateScript(EntitySet entitySet)
		{
			EntityType e = entitySet.ElementType;
			_guidIdentityColumns = new List<string>();

			string typeName = null;
			if (_pluralizedNames.ContainsKey(e.Name))
			{
				typeName = _pluralizedNames[e.Name];
			}
			else
			{
				_pluralizedNames.Add(e.Name,
					(string)entitySet.MetadataProperties["Table"].Value == null ?
					e.Name : (string)entitySet.MetadataProperties["Table"].Value);
				typeName = _pluralizedNames[e.Name];
			}

			StringBuilder sql = new StringBuilder("CREATE TABLE ");
			sql.AppendFormat("`{0}`(", typeName);
			string delimiter = "";
			foreach (EdmProperty c in e.Properties)
			{
				sql.AppendFormat("{0}{1}\t`{2}` {3}{4}", delimiter, Environment.NewLine, c.Name,
						GetColumnType(c.TypeUsage), GetFacetString(c, e.KeyMembers.Contains(c.Name)));
				delimiter = ", ";
			}
			sql.AppendLine(");");
			sql.AppendLine();
            //if (e.KeyMembers.Count > 0)
            //{
            //             if (e.KeyMembers.Count !=1 || e.KeyMembers.Count ==1 && !uniquenKeys.Contains(e.KeyMembers.First().Name))
            //             {
            //		sql.Append(String.Format(
            //			"ALTER TABLE `{0}` ADD PRIMARY KEY (", typeName));
            //		delimiter = "";
            //		foreach (EdmMember m in e.KeyMembers)
            //		{
            //			sql.AppendFormat("{0}`{1}`", delimiter, m.Name);
            //			delimiter = ", ";
            //		}
            //		sql.AppendLine(");");
            //		sql.AppendLine();
            //	}
            //}
            if (_guidIdentityColumns.Count > 0)
            {
				//sql.AppendLine(string.Format("CREATE TRIGGER `{0}` BEFORE INSERT ON `{1}`", typeName + "_IdentityTgr", typeName));
				//sql.AppendLine("\tFOR EACH ROW BEGIN");
				//sql.AppendLine(string.Format("\tDECLARE var_{0} CHAR(36);", _guidIdentityColumns[0]));
				//sql.AppendLine("BEGIN");
				//foreach (string guidColumn in _guidIdentityColumns)
				//{
				//    if (e.KeyMembers.Contains(guidColumn))
				//    {
				//        sql.AppendLine(string.Format("\t\tDROP TEMPORARY TABLE IF EXISTS tmpIdentity_{0};", typeName));
				//        sql.AppendLine(string.Format("\t\tCREATE TEMPORARY TABLE tmpIdentity_{0} (guid CHAR(36));", typeName));
				//        sql.AppendLine(string.Format("\t\tvar_{0} := sys_guid;", guidColumn));
				//        sql.AppendLine(string.Format("\t\tINSERT INTO tmpIdentity_{0} VALUES(var_{1});", typeName, guidColumn));
				//        sql.AppendLine(string.Format("\t\tnew.{0} := var_{0};", guidColumn));
				//    }
				//    else
				//        sql.AppendLine(string.Format("\t\tnew.{0} := UUID();", guidColumn));
				//}
				//sql.AppendLine("\tEND");
				var names = typeName.Split('.');
				string tableName=names.Length>1?names[1]:typeName;
				string schemaName=names.Length>1?names[0]:"SYSDBA";
				sql.AppendLine(string.Format("DROP TABLE IF EXISTS `{0}`.`tmpIdentity_{1}`;", schemaName, tableName));
				sql.AppendLine(string.Format("CREATE TABLE `{0}`.`tmpIdentity_{1}` (`guid` guid);", schemaName, tableName));
				sql.AppendLine(string.Format("DROP TRIGGER IF EXISTS `{0}`.`{1}_IdentityTgr`;", schemaName, tableName));
				sql.AppendLine(string.Format("CREATE TRIGGER `{0}`.`{1}_IdentityTgr` BEFORE INSERT ON `{0}`.`{1}`", schemaName, tableName));
				sql.AppendLine("FOR EACH ROW BEGIN");
				for (int i = 0; i < _guidIdentityColumns.Count; i++)
				{
					sql.AppendLine(string.Format("NEW.{0} := sys_guid();", _guidIdentityColumns[i]));
				}
				sql.AppendLine(string.Format("INSERT INTO `{0}`.`tmpIdentity_{1}` VALUES(New.{3});", schemaName, tableName, _guidIdentityColumns.FirstOrDefault(i=>e.KeyMembers.Contains(i))));
				sql.AppendLine("END;");
			}
            sql.AppendLine();
            return sql.ToString();
		}

		internal static string GetSchemaName(EntitySet entitySet)
		{
			return entitySet.GetMetadataPropertyValue<string>("Schema") ?? entitySet.EntityContainer.Name;
		}
		internal static string GetTableName(EntitySet entitySet)
		{
			return entitySet.GetMetadataPropertyValue<string>("Table") ?? entitySet.Name;
		}
		internal string GetColumnType(TypeUsage type)
		{
			string t = type.EdmType.Name;
			if (t.StartsWith("u", StringComparison.OrdinalIgnoreCase))
			{
				t = t.Substring(1).ToUpperInvariant();// + " UNSIGNED";
			}
			//else if (String.Compare(t, "guid", true) == 0)
			//	return "CHAR(36) BINARY";
			return t;
		}

		private string GetFacetString(EdmProperty column, bool IsKeyMember)
		{
			StringBuilder sql = new StringBuilder();
			Facet facet;
			Facet fcDateTimePrecision = null;

			ReadOnlyMetadataCollection<Facet> facets = column.TypeUsage.Facets;

			if (column.TypeUsage.EdmType.BaseType.Name == "String")
			{
				// types tinytext, mediumtext, text & longtext don't have a length.
				if (!column.TypeUsage.EdmType.Name.EndsWith("clob", StringComparison.OrdinalIgnoreCase))
				{
					if (facets.TryGetValue("MaxLength", true, out facet))
					{
						sql.AppendFormat(" ({0})", facet.Value);
					}
				}
			}
			else if (column.TypeUsage.EdmType.BaseType.Name == "Decimal")
			{
				Facet fcScale;
				Facet fcPrecision;
				if (facets.TryGetValue("Scale", true, out fcScale) && facets.TryGetValue("Precision", true, out fcPrecision))
				{
					// Enforce scale to a reasonable value.
					int scale = fcScale.Value == null ? 0 : (int)(byte)fcScale.Value;
					if (scale == 0)
						scale = XGProviderManifest.DEFAULT_DECIMAL_SCALE;
					sql.AppendFormat("( {0}, {1} ) ", fcPrecision.Value, scale);
				}
			}

			if (facets.TryGetValue("StoreGeneratedPattern", true, out facet))
			{
				if (facet.Value.Equals(StoreGeneratedPattern.Identity))
				{

					if (column.TypeUsage.EdmType.BaseType.Name.StartsWith("Int"))
                    {
						sql.Append(" IDENTITY PRIMARY KEY ");

						//uniquenKeys.Add(column.Name);
                    }
					else if (column.TypeUsage.EdmType.BaseType.Name == "Guid")
						_guidIdentityColumns.Add(column.Name);
					else if (column.TypeUsage.EdmType.BaseType.Name == "DateTime")
						sql.AppendFormat(" DEFAULT CURRENT_TIMESTAMP{0}", fcDateTimePrecision != null && Convert.ToByte(fcDateTimePrecision.Value) >= 1 ? "( " + fcDateTimePrecision.Value.ToString() + " )" : "");
					else
						throw new Exception("Invalid identity column type.");
				}
				else if (facet.Value.Equals(StoreGeneratedPattern.Computed))
				{
					if (column.TypeUsage.EdmType.BaseType.Name == "DateTime")
						sql.AppendFormat(" DEFAULT CURRENT_TIMESTAMP{0}", fcDateTimePrecision != null && Convert.ToByte(fcDateTimePrecision.Value) >= 1 ? "( " + fcDateTimePrecision.Value.ToString() + " )" : "");
				}
			}

			if (facets.TryGetValue("Nullable", true, out facet) && (bool)facet.Value == false)
				sql.Append(" NOT NULL");
			return sql.ToString();
		}

		private bool IsStringType(TypeUsage type)
		{
			return false;
		}
	}
}
