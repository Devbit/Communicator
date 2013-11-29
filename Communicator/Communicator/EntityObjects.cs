using System.Collections.Generic;

namespace Communicator
{
    public class EntityObjects
    {
    }

    public class Profile
    {
        public string _id { get; set; }
        public string profile_id { get; set; }
        public string url { get; set; }
        public string sector { get; set; }
        public Name name { get; set; }
        public string picture { get; set; }
        public string overview_html { get; set; }
        public string specialties { get; set; }
        public List<Language> languages { get; set; }
        public List<string> skills { get; set; }
        public string interests { get; set; }
        public Group groups { get; set; }
        public List<string> honors { get; set; }
        public List<Education> education { get; set; }
        public List<Experience> experience { get; set; }
        public List<Course> courses { get; set; }
    }

    public class Name
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
    }

    public class Language
    {
        public string name { get; set; }
        public string level { get; set; }
    }

    public class Group
    {
        public string[] groups { get; set; }
        public string[] organisations { get; set; }
    }

    public class Education
    {
        public string title { get; set; }
        public string degree { get; set; }
        public string major { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string description { get; set; }
    }

    public class Experience
    {
        public string title { get; set; }
        public string company { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string location { get; set; }
        public string description { get; set; }
        public string details { get; set; }
    }

    public class Course
    {
        public string organisation { get; set; }
        public string title { get; set; }
        public string[] competency { get; set; }
    }

    public class Vacancy
    {
        public string _id { get; set; }
        public string source { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string company { get; set; }
        public Contact contact { get; set; }
        public Details details { get; set; }
        public string logo { get; set; }
        public string date_created { get; set; }
        public string date_updated { get; set; }
    }

    public class Contact
    {
        public string email { get; set; }
        public string phone { get; set; }
        public string person { get; set; }
        public Address address { get; set; }
    }

    public class Address
    {
        public string city { get; set; }
        public string company { get; set; }
        public string street { get; set; }
        public string postal { get; set; }
    }

    public class Details
    {
        public string salary { get; set; }
        public string job_type { get; set; }
        public string hours { get; set; }
        public string education_level { get; set; }
        public string career_level { get; set; }
        public string advert_html { get; set; }
        public string location { get; set; }
    }

}