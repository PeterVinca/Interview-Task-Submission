using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerRequestProcessing.Core
{
    public interface IEntity {}

    public interface IEntityValidator<IEntity>
    {
        IEnumerable<string> Validate(IEntity entity);
    }
}
