using System.Runtime.Serialization;

namespace AdsConnect.api
{
    public class ResponseData<T>
    {
        private T? _data;
        private string? _errMessage;
        private bool _success;

        [DataMember()]
        public T? data
        {
            get { return _data; }
            set { _data = value; }
        }

        [DataMember()]
        public string? errMessage
        {
            get { return _errMessage; }
            set { _errMessage = value; }
        }

        [DataMember()]
        public bool success
        {
            get { return _success; }
            set { _success = value; }
        }
    }
}
