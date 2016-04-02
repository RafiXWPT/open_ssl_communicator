using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class DisplayContact
    {

        public string DisplayStatus { get { return GetStatus(); } }
        public string ContactID { get; }
        public string DisplayName { get; set; }

        public DisplayContact(string ID, string displayName)
        {
            ContactID = ID;
            DisplayName = displayName;
        }

        string GetStatus()
        {
            string uri = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\";
            
            string status = StatusController.Instance.GetUserStatus(ContactID);
            uri += "status" + status + ".png";

            return new Uri(uri, UriKind.RelativeOrAbsolute).ToString();
        }
    }
}
