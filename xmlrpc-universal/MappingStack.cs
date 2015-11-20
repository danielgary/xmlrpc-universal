using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Data.Xml.Rpc
{
    public class MappingStack : Stack<string>
    {
      public MappingStack(string parseType)
      {
        m_parseType = parseType;
      }

      void Push(string str)
      {
        base.Push(str);
      }

      public string MappingType
      {
        get { return m_parseType; }
      }

      public string m_parseType = "";
    }
  }
