using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_SetTableAttribute : StoredProcedure, IStoredProc, IHelper
    {
        void SetValue(string tableName, string attrName, string value)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            switch (attrName.ToLower())
            {
                case "indexonly":
                    {
                        bool indexonly;

                        if (bool.TryParse(value, out indexonly))
                        {
                            dbProvider.SetIndexOnly(indexonly);
                            //dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} index only to {1} sucessful!",
                                tableName, dbProvider.IndexOnly));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;
                case "debug":
                    {
                        bool debug;

                        if (bool.TryParse(value, out debug))
                        {
                            dbProvider.Table.Debug = debug;
                            
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} debug to {1} sucessful!",
                                tableName, dbProvider.Table.Debug));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;
                case "initimmediatelyafterstartup":
                    {
                        bool initimmediatelyafterstartup;

                        if (bool.TryParse(value, out initimmediatelyafterstartup))
                        {
                            dbProvider.Table.InitImmediatelyAfterStartup = initimmediatelyafterstartup;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} InitImmediatelyAfterStartup to {1} sucessful!",
                                tableName, dbProvider.Table.InitImmediatelyAfterStartup));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;


                case "querycacheenabled":
                    {
                        bool querycacheenabled;

                        if (bool.TryParse(value, out querycacheenabled))
                        {
                            dbProvider.SetCacheQuery(querycacheenabled, dbProvider.QueryCacheTimeout);
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} QueryCacheEnabled to {1} sucessful!",
                                tableName, dbProvider.QueryCacheEnabled));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;
                case "querycachetimeout":
                    {
                        int querycachetimeout;

                        if (int.TryParse(value, out querycachetimeout))
                        {
                            dbProvider.SetCacheQuery(dbProvider.QueryCacheEnabled, querycachetimeout);
                            dbProvider.SaveTable();

                            OutputMessage(string.Format("Set table {0} QueryCacheTimeout to {1} sucessful!",
                                tableName, dbProvider.QueryCacheTimeout));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be number");
                        }
                    }
                    break;
                case "storequerycacheinfile":
                    {
                        bool storequerycacheinfile;

                        if (bool.TryParse(value, out storequerycacheinfile))
                        {
                            dbProvider.SetStoreQueryCacheInFile(storequerycacheinfile);
                            //dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} StoreQueryCacheInFile to {1} sucessful!",
                                tableName, dbProvider.Table.StoreQueryCacheInFile));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;
                case "cleanupquerycachefileindays":
                    {
                        int cleanupquerycachefileindays;

                        if (int.TryParse(value, out cleanupquerycachefileindays))
                        {
                            dbProvider.Table.CleanupQueryCacheFileInDays = cleanupquerycachefileindays;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} CleanupQueryCacheFileInDays to {1} sucessful!",
                                tableName, dbProvider.Table.CleanupQueryCacheFileInDays));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be number");
                        }
                    }
                    break;
                case "maxreturncount":
                    {
                        int count;

                        if (int.TryParse(value, out count))
                        {
                            dbProvider.SetMaxReturnCount(count);
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} max return count to {1} sucessful!",
                                tableName, dbProvider.MaxReturnCount));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be number");
                        }
                    }
                    break;
                case "groupbylimit":
                    {
                        int count;

                        if (int.TryParse(value, out count))
                        {
                            dbProvider.Table.GroupByLimit = count;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} GroupByLimit to {1} sucessful!",
                                tableName, dbProvider.Table.GroupByLimit));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be number");
                        }
                    }
                    break;
                case "indexthread":
                    {
                        int indexthread;

                        if (int.TryParse(value, out indexthread))
                        {
                            dbProvider.Table.IndexThread = indexthread;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} index thread to {1} sucessful!",
                                tableName, dbProvider.Table.IndexThread));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be number");
                        }
                    }
                    break;
                case "tablesynchronization":
                    {
                        bool tablesynchronization;

                        if (bool.TryParse(value, out tablesynchronization))
                        {
                            dbProvider.Table.TableSynchronization = tablesynchronization;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} TableSynchronization to {1} sucessful!",
                                tableName, dbProvider.Table.TableSynchronization));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;

                case "triggertablename":
                    {
                        dbProvider.Table.TriggerTableName = value;

                        dbProvider.SaveTable();
                        OutputMessage(string.Format("Set table {0} TriggerTableName to {1} sucessful!",
                            tableName, dbProvider.Table.TriggerTableName));
                    }
                    break;
                case "mirrortableenabled":
                    {
                        bool mirrortableenabled;

                        if (bool.TryParse(value, out mirrortableenabled))
                        {
                            dbProvider.Table.MirrorTableEnabled = mirrortableenabled;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} MirrorTableEnabled to {1} sucessful!",
                                tableName, dbProvider.Table.MirrorTableEnabled));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }

                    break;
                case "usingmirrortablefornonfulltextquery":
                    {
                        bool usingmirrortablefornonfulltextquery;

                        if (bool.TryParse(value, out usingmirrortablefornonfulltextquery))
                        {
                            dbProvider.Table.UsingMirrorTableForNonFulltextQuery = usingmirrortablefornonfulltextquery;
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} UsingMirrorTableForNonFulltextQuery to {1} sucessful!",
                                tableName, dbProvider.Table.UsingMirrorTableForNonFulltextQuery));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;
                default:
                    throw new StoredProcException("Can't set attribute:{0}, it is only can set at create statement");
            }

        }

        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_SetTableAttribute";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo(Right.RightItem.ManageDB);

            if (Parameters.Count != 3)
            {
                throw new StoredProcException("First parameter is table name. Second parameter is attribute name. Third is value");
            }

            string tableName = Parameters[0];
            string attrName = Parameters[1];
            string value = Parameters[2];

            SetValue(tableName, attrName, value);
        }

        #endregion


        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Set table attribute. First parameter is table name. Second parameter is attribute name. Third is value";
            }
        }

        #endregion
    }
}
