using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public interface IDataMapper<S, E, T>
    {
        E Map(S request);

        T Map(E entity);
    }
}
