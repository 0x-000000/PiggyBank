using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;

namespace Bank.Models
{
    public class Credentials
    {
        private readonly string xmlPath;

        public Credentials()
        {
            xmlPath = HostingEnvironment.MapPath("~/App_Data/Member.xml");

            if (string.IsNullOrEmpty(xmlPath))
            {
                throw new Exception("cant figure out credential file path");
            }

            var folder = Path.GetDirectoryName(xmlPath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(xmlPath))
            {
                File.WriteAllText(xmlPath, "<Members />");
            }
        }

        public bool Validate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var doc = LoadDocument();
            var hash = Hash(password);
            var members = doc.Root == null ? Enumerable.Empty<XElement>() : doc.Root.Elements("Member");

            foreach (var member in members)
            {
                var name = (string)member.Element("Username");
                if (name != null && name.Equals(username.Trim()))
                {
                    var stored = (string)member.Element("PasswordHash");
                    return string.Equals(stored, hash);
                }
            }

            return false;
        }

        public void WriteCredential(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                throw new Exception("bad username or password");
            }

            var doc = LoadDocument();
            var members = doc.Root ?? new XElement("Members");
            if (doc.Root == null)
            {
                doc.Add(members);
            }

            XElement found = null;
            foreach (var member in members.Elements("Member"))
            {
                var name = (string)member.Element("Username");
                if (name != null && name.Equals(username.Trim()))
                {
                    found = member;
                    break;
                }
            }

            var hash = Hash(password);

            if (found == null)
            {
                var newMember = new XElement("Member",
                    new XElement("Username", username.Trim()),
                    new XElement("PasswordHash", hash));
                members.Add(newMember);
            }
            else
            {
                var pass = found.Element("PasswordHash");
                if (pass == null)
                {
                    found.Add(new XElement("PasswordHash", hash));
                }
                else
                {
                    pass.Value = hash;
                }
            }

            doc.Save(xmlPath);
        }

        public bool HasUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var doc = LoadDocument();
            var members = doc.Root == null ? Enumerable.Empty<XElement>() : doc.Root.Elements("Member");
            foreach (var member in members)
            {
                var name = (string)member.Element("Username");
                if (name != null && name.Equals(username.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        private XDocument LoadDocument()
        {
            try
            {
                return XDocument.Load(xmlPath);
            }
            catch
            {
                var doc = new XDocument(new XElement("Members"));
                doc.Save(xmlPath);

                return doc;
            }
        }

        private static string Hash(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hash)
                {
                    // store hex of hash
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
