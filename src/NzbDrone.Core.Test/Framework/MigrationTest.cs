using System;
using FluentMigrator;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Test.Framework
{
    [Category("DbMigrationTest")]
    [Category("DbTest")]
    public abstract class MigrationTest<TMigration> : DbTest
        where TMigration : NzbDroneMigrationBase
    {
        protected long MigrationVersion
        {
            get
            {
                var attrib = (MigrationAttribute)Attribute.GetCustomAttribute(typeof(TMigration), typeof(MigrationAttribute));
                return attrib.Version;
            }
        }

        protected virtual IDirectDataMapper WithMigrationTestDb(Action<TMigration> beforeMigration = null)
        {
            var db = WithTestDb(new MigrationContext(MigrationType, MigrationVersion)
            {
                BeforeMigration = m =>
                {
                    var migration = m as TMigration;
                    if (beforeMigration != null && migration != null)
                    {
                        beforeMigration(migration);
                    }
                }
            });

            return db.GetDirectDataMapper();
        }

        protected override void SetupLogging()
        {
            Mocker.SetConstant<ILoggerProvider>(Mocker.Resolve<NLogLoggerProvider>());
        }

        [SetUp]
        public override void SetupDb()
        {
            SetupContainer();
        }
    }
}
