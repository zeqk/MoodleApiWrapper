﻿using MoodleApiWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoodleApiWrapper
{
    public class ApiWrapper
    {
        /// <summary>
        /// field that holds your api token
        /// </summary>
        private static string _apiToken;
        /// <summary>
        /// This property sets you Api token.
        /// </summary>
        public static string ApiToken
        {
            get { return _apiToken; }
            set { _apiToken = value; }
        }

        /// <summary>
        /// Repressents if the token is set.
        /// </summary>
        private static bool TokenIsSet => ApiToken.Any();

        private static Uri _host;
        public static Uri Host
        {
            get { return _host; }
            set { _host = value; }
        }

        /// <summary>
        /// Represents if the host address is set.
        /// </summary>
        private static bool HostIsSet => Host.AbsoluteUri.Any();


        private static string ParseFormat(Format format)
        {
            switch (format)
            {
                case Format.JSON:
                    return "json";
                case Format.XML:
                    return "xml";
            }
            throw new ArgumentOutOfRangeException("format");
        }

        private static string ParseMethod(Methods method)
        {
            switch (method)
            {
                case Methods.core_webservice_get_site_info:
                    return "core_webservice_get_site_info";
                case Methods.core_user_get_users:
                    return "core_user_get_users";
                case Methods.core_user_get_users_by_field:
                    return "core_user_get_users_by_field";
                case Methods.core_enrol_get_users_courses:
                    return "core_enrol_get_users_courses";
                case Methods.core_user_create_users:
                    return "core_user_create_users"; 
                case Methods.core_user_update_users:
                    return "core_user_update_users";
                case Methods.core_user_delete_users:
                    return "core_user_delete_users";
                case Methods.core_role_assign_roles:
                    return "core_role_assign_roles";
                case Methods.core_role_unassign_roles:
                    return "core_role_unassign_roles";
                case Methods.enrol_manual_enrol_users:
                    return "enrol_manual_enrol_users";
                case Methods.core_group_add_group_members:
                    return "core_group_add_group_members";
                case Methods.core_group_delete_group_members:
                    return "core_group_delete_group_members";
                case Methods.core_course_get_categories:
                    return "core_course_get_categories";
                case Methods.core_course_get_courses:
                    return "core_course_get_courses";
                case Methods.core_course_get_contents:
                    return "core_course_get_contents";
                case Methods.core_group_get_groups:
                    return "core_group_get_groups";
                case Methods.core_group_get_course_groups:
                    return "core_group_get_course_groups";
                case Methods.core_enrol_get_enrolled_users:
                    return "core_enrol_get_enrolled_users";
                case Methods.core_course_create_courses:
                    return "core_course_create_courses";
                case Methods.core_course_update_courses:
                    return "core_course_update_courses";
                case Methods.core_grades_get_grades:
                    return "core_grades_get_grades";
                case Methods.core_grades_update_grades:
                    return "core_grades_update_grades";
                case Methods.core_grading_get_definitions:
                    return "core_grading_get_definitions";
                case Methods.core_calendar_get_calendar_events:
                    return "core_calendar_get_calendar_events";
                case Methods.core_calendar_create_calendar_events:
                    return "core_calendar_create_calendar_events";
                case Methods.core_calendar_delete_calendar_events:
                    return "core_calendar_delete_calendar_events";
                case Methods.default_:
                    return "";
            }
            throw new ArgumentOutOfRangeException("method");
        }


        #region functions
        /// <summary>
        /// Returns your Api Token needed to make any calls
        /// <para />
        /// service shortname - The service shortname is usually hardcoded in the pre-build service (db/service.php files).
        /// Moodle administrator will be able to edit shortnames for service created on the fly: MDL-29807.
        /// If you want to use the Mobile service, its shortname is moodle_mobile_app. Also useful to know,
        /// the database shortname field can be found in the table named external_services.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="serviceHostName"></param>
        /// <returns></returns>
        public static Task<AuthentiactionResponse<AuthToken>> GetApiToken(string username, string password ,string serviceHostName)
        {
            if (HostIsSet)
            {
                string query = 
                    "login/token.php" +
                    $"?username={username}" +
                    $"&password={password}" +
                    $"&service={serviceHostName}";

                return GetAuth<AuthToken>(Host.AbsoluteUri + query);
            }
            else
            {
                throw  new Exception("Host is not set");
            }
        }


        /// <summary>
        /// This API will return information about the site, web services users, and authorized API actions. This call is useful for getting site information and the capabilities of the web service user. 
        /// </summary>
        /// <param name="serviceHostNames">Returns information about a particular service.</param>
        /// <returns></returns>
        public static Task<ApiResponse<Site_info>> GetSiteInfo(string serviceHostName = "")
        {
            if (HostIsSet && TokenIsSet)
            {
                var query = string.Empty;
                if (!serviceHostName.Any())
                {
                    query = string.Format("webservice/rest/server.php" +
                       "?wstoken={0}&moodlewsrestformat={1}&wsfunction={2}",
                       ApiToken, ParseFormat(Format.JSON), ParseMethod(Methods.core_webservice_get_site_info));
                }
                else
                {
                    query = string.Format("webservice/rest/server.php" +
                         "?wstoken={0}&moodlewsrestformat={1}&wsfunction={2}&serviceshortnames[0]={3}",
                         ApiToken, ParseFormat(Format.JSON), ParseMethod(Methods.core_webservice_get_site_info), serviceHostName);
                }

                return Get<Site_info>(Host.AbsoluteUri + query);
            }
            else
            {
                if (!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if (!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }
        }
        /// <summary>
        /// Search for users matching the parameters of the call. This call will return matching user accounts with profile fields.
        ///  The key/value pairs to be considered in user search. Values can not be empty. Specify different keys only once
        ///  (fullname =&gt; 'user1', auth =&gt; 'manual', ...) - key occurences are forbidden. The search is executed with AND operator on the criterias.
        ///  Invalid criterias (keys) are ignored, the search is still executed on the valid criterias. You can search without criteria,
        ///  but the function is not designed for it. It could very slow or timeout. The function is designed to search some specific users.
        /// <para />
        /// "id" (int) matching user id<para />
        ///"lastname" (string) user last name (Note: you can use % for searching but it may be considerably slower!)<para />
        ///"firstname" (string) user first name (Note: you can use % for searching but it may be considerably slower!)<para />
        ///"idnumber" (string) matching user idnumber<para />
        ///"username" (string) matching user username<para />
        ///"email" (string) user email (Note: you can use % for searching but it may be considerably slower!)<para />
        ///"auth" (string) matching user auth plugin<para />
        /// </summary>
        /// <param name="criteria_key0">Key of the first search parameter.</param>
        /// <param name="criteria_value0">Value of the first search term.</param>
        /// <param name="criteria_key1">Key of the second search parameter.</param>
        /// <param name="criteria_value1">Value of the second search term.</param>
        /// <returns></returns>
        public static Task<ApiResponse<Users>> GetUsers(string criteria_key0, string criteria_value0, string criteria_key1 = "", string criteria_value1= "")
        {
            if (HostIsSet && TokenIsSet)
            {
                string query = string.Empty;
                if (criteria_key1.Any() && criteria_value1.Any())
                {
                    query =
                        "webservice/rest/server.php?" +
                        $"wstoken={ApiToken}&" +
                        $"wsfunction={ParseMethod(Methods.core_user_get_users)}&" +
                        $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                        $"criteria[0][key]={criteria_key0}&" +
                        $"criteria[0][value]={criteria_value0}" +
                        $"criteria[1][key]={criteria_key0}&" +
                        $"criteria[1][value]={criteria_value0}";
                }
                else
                {
                    query =
                        "webservice/rest/server.php?" +
                        $"wstoken={ApiToken}&" +
                        $"wsfunction={ParseMethod(Methods.core_user_get_users)}&" +
                        $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                        $"criteria[0][key]={criteria_key0}&" +
                        $"criteria[0][value]={criteria_value0}";
                }
                return Get<Users>(Host.AbsoluteUri + query);


            }
            else
            {
                if (!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if (!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }
        }
        /// <summary>
        /// Retrieve users information for a specified unique field - If you want to do a user search, use GetUsers()
        /// 
        /// Avaiable Criteria:
        ///"id" (int) matching user id
        ///"lastname" (string) user last name (Note: you can use % for searching but it may be considerably slower!)
        ///"firstname" (string) user first name (Note: you can use % for searching but it may be considerably slower!)
        ///"idnumber" (string) matching user idnumber
        ///"username" (string) matching user username
        ///"email" (string) user email (Note: you can use % for searching but it may be considerably slower!)
        ///"auth" (string) matching user auth plugin
        /// </summary>
        /// <param name="criteria_key">Key of the first search parameter.</param>
        /// <param name="criteria_value">Value of the first search term.</param>
        /// <returns></returns>
        public static Task<ApiResponse<Users>> GetUsersByField(string criteria_key, string criteria_value)
        {
            if (HostIsSet && TokenIsSet)
            {
                string query = string.Empty;
                       query =
                        "webservice/rest/server.php?" +
                        $"wstoken={ApiToken}&" +
                        $"wsfunction={ParseMethod(Methods.core_user_get_users_by_field)}&" +
                        $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                        $"criteria[0][key]={criteria_key}&" +
                        $"criteria[0][value]={criteria_value}";
                
                return Get<Users>(Host.AbsoluteUri + query);
            }
            else
            {
                if (!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if (!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }
        }
        /// <summary>
        /// Get the list of courses where a user is enrolled in 
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static Task<ApiResponse<Cources>> GetUserCourses(int userid)
        {
            if (HostIsSet && TokenIsSet)
            {
                string query = string.Empty;
                query =
                    "webservice/rest/server.php?" +
                    $"wstoken={ApiToken}&" +
                    $"wsfunction={ParseMethod(Methods.core_enrol_get_users_courses)}&" +
                    $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                    $"userid={userid}";

                return Get<Cources>(Host.AbsoluteUri + query);
            }
            else
            {
                if(!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if(!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }
        }
        /// <summary>
        /// Create a User.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="auth"></param>
        /// <param name="idnumber"></param>
        /// <param name="lang"></param>
        /// <param name="calendartye"></param>
        /// <param name="theme"></param>
        /// <param name="timezone"></param>
        /// <param name="mailformat"></param>
        /// <param name="description"></param>
        /// <param name="city"></param>
        /// <param name="country"></param>
        /// <param name="firstnamephonetic"></param>
        /// <param name="lastnamephonetic"></param>
        /// <param name="middlename"></param>
        /// <param name="alternatename"></param>
        /// <param name="preferences_type"></param>
        /// <param name="preferences_value"></param>
        /// <param name="customfields_type"></param>
        /// <param name="customfields_value"></param>
        /// <returns></returns>
        public static Task<ApiResponse<NewUser>> CreateUser(string username,string firstname,string lastname,string email, string password,
            string auth = "", string idnumber = "",string lang = "" , string calendartye = "" , string theme = "", string timezone= "",
            string mailformat = "", string description = "", string city ="", string country = "", string firstnamephonetic = "",
            string lastnamephonetic = "", string middlename = "", string alternatename = "", string preferences_type = "", string preferences_value = "",
            string customfields_type = "", string customfields_value = "")
        {
            if (HostIsSet && TokenIsSet)
            {
                StringBuilder querybuilder = new StringBuilder();
                querybuilder.Append(
                    "webservice/rest/server.php?" +
                    $"wstoken={ApiToken}&wsfunction={ParseMethod(Methods.core_user_create_users)}&" +
                    $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                    $"users[0][username]={@username}&" +
                    $"users[0][password]={@password}&" +
                    $"users[0][firstname]={@firstname}&" +
                    $"users[0][lastname]={@lastname}&" +
                    $"users[0][email]={@email}");
                if (auth.Any()) querybuilder.Append($"&users[0][auth]={auth}");
                if (idnumber.Any()) querybuilder.Append($"&users[0][auth]={idnumber}");
                if (lang.Any()) querybuilder.Append($"&users[0][auth]={lang}");
                if (calendartye.Any()) querybuilder.Append($"&users[0][auth]={calendartye}");
                if (theme.Any()) querybuilder.Append($"&users[0][auth]={theme}");
                if (timezone.Any()) querybuilder.Append($"&users[0][auth]={timezone}");
                if (mailformat.Any()) querybuilder.Append($"&users[0][auth]={mailformat}");
                if (description.Any()) querybuilder.Append($"&users[0][auth]={description}");
                if (city.Any()) querybuilder.Append($"&users[0][auth]={city}");
                if (country.Any()) querybuilder.Append($"&users[0][auth]={country}");
                if (firstnamephonetic.Any()) querybuilder.Append($"&users[0][auth]={firstnamephonetic}");
                if (lastnamephonetic.Any()) querybuilder.Append($"&users[0][auth]={lastnamephonetic}");
                if (middlename.Any()) querybuilder.Append($"&users[0][auth]={middlename}");
                if (alternatename.Any()) querybuilder.Append($"&users[0][auth]={alternatename}");
                if (preferences_type.Any()) querybuilder.Append($"&users[0][auth]={preferences_type}");
                if (preferences_value.Any()) querybuilder.Append($"&users[0][auth]={preferences_value}");
                if (customfields_type.Any()) querybuilder.Append($"&users[0][auth]={customfields_type}");
                if (customfields_value.Any()) querybuilder.Append($"&users[0][auth]={customfields_value}");


                return Get<NewUser>(Host.AbsoluteUri + querybuilder.ToString());
            }
            else
            {
                if (!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if (!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }
        }


        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="auth"></param>
        /// <param name="idnumber"></param>
        /// <param name="lang"></param>
        /// <param name="calendartye"></param>
        /// <param name="theme"></param>
        /// <param name="timezone"></param>
        /// <param name="mailformat"></param>
        /// <param name="description"></param>
        /// <param name="city"></param>
        /// <param name="country"></param>
        /// <param name="firstnamephonetic"></param>
        /// <param name="lastnamephonetic"></param>
        /// <param name="middlename"></param>
        /// <param name="alternatename"></param>
        /// <param name="preferences_type"></param>
        /// <param name="preferences_value"></param>
        /// <param name="customfields_type"></param>
        /// <param name="customfields_value"></param>
        /// <returns></returns>
        public static Task<ApiResponse<NewUser>> UpdateUser(int id,string username ="", string firstname = "", string lastname = "",
           string email = "", string password = "",string auth = "", string idnumber = "", string lang = "", string calendartye = "", string theme = "",
           string timezone = "", string mailformat = "", string description = "", string city = "", string country = "", string firstnamephonetic = "",
           string lastnamephonetic = "", string middlename = "", string alternatename = "", string preferences_type = "", string preferences_value = "",
           string customfields_type = "", string customfields_value = "")
        {
            if (HostIsSet && TokenIsSet)
            {
                StringBuilder querybuilder = new StringBuilder();
                querybuilder.Append(
                    "webservice/rest/server.php?" +
                    $"wstoken={ApiToken}&wsfunction={ParseMethod(Methods.core_user_create_users)}&" +
                    $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                    $"users[0][id]={id}");

                if (username.Any()) querybuilder.Append($"&users[0][username]={username}");
                if (password.Any()) querybuilder.Append($"&users[0][password]={password}");
                if (firstname.Any()) querybuilder.Append($"&users[0][firstname]={firstname}");
                if (lastname.Any()) querybuilder.Append($"&users[0][lastname]={lastname}"); 
                if (email.Any()) querybuilder.Append($"&users[0][email]={email}");
                if (auth.Any()) querybuilder.Append($"&users[0][auth]={auth}");
                if (idnumber.Any()) querybuilder.Append($"&users[0][auth]={idnumber}");
                if (lang.Any()) querybuilder.Append($"&users[0][auth]={lang}");
                if (calendartye.Any()) querybuilder.Append($"&users[0][auth]={calendartye}");
                if (theme.Any()) querybuilder.Append($"&users[0][auth]={theme}");
                if (timezone.Any()) querybuilder.Append($"&users[0][auth]={timezone}");
                if (mailformat.Any()) querybuilder.Append($"&users[0][auth]={mailformat}");
                if (description.Any()) querybuilder.Append($"&users[0][auth]={description}");
                if (city.Any()) querybuilder.Append($"&users[0][auth]={city}");
                if (country.Any()) querybuilder.Append($"&users[0][auth]={country}");
                if (firstnamephonetic.Any()) querybuilder.Append($"&users[0][auth]={firstnamephonetic}");
                if (lastnamephonetic.Any()) querybuilder.Append($"&users[0][auth]={lastnamephonetic}");
                if (middlename.Any()) querybuilder.Append($"&users[0][auth]={middlename}");
                if (alternatename.Any()) querybuilder.Append($"&users[0][auth]={alternatename}");
                if (preferences_type.Any()) querybuilder.Append($"&users[0][auth]={preferences_type}");
                if (preferences_value.Any()) querybuilder.Append($"&users[0][auth]={preferences_value}");
                if (customfields_type.Any()) querybuilder.Append($"&users[0][auth]={customfields_type}");
                if (customfields_value.Any()) querybuilder.Append($"&users[0][auth]={customfields_value}");


                return Get<NewUser>(Host.AbsoluteUri + querybuilder.ToString());
            }
            else
            {
                if (!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if (!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }
        }

        public static Task<ApiResponse<NewUser>> DeleteUser(int id)
        {
            if (HostIsSet && TokenIsSet)
            {
                string query = string.Empty;
                query =
                 "webservice/rest/server.php?" +
                 $"wstoken={ApiToken}&" +
                 $"wsfunction={ParseMethod(Methods.core_user_get_users_by_field)}&" +
                 $"moodlewsrestformat={ParseFormat(Format.JSON)}&" +
                 $"userids[0]={id}";

                return Get<NewUser>(Host.AbsoluteUri + query);
            }
            else
            {
                if (!HostIsSet && TokenIsSet)
                    throw new Exception("Host & token are not set");
                else if (!HostIsSet)
                    throw new Exception("Host is not set");
                else
                    throw new Exception("Token is not set");
            }

        }
        private static async Task<AuthentiactionResponse<T>> GetAuth<T>(string uri) where T : IDataModel
        {
            try
            {
                var request = WebRequest.Create(Uri.EscapeUriString(uri));
                using (var response = await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var data = JObject.Parse(await reader.ReadToEndAsync());
                       return new AuthentiactionResponse<T>(new AuthentiactionResponseRaw(data));
                    }
                }
            }
            catch (WebException)
            {
                // No internet connection
                throw new WebException("No internet connection.");
            }
        }

        private static async Task<ApiResponse<T>> Get<T>(string uri) where T : IDataModel
        {
            try
            {
                var request = WebRequest.Create(Uri.EscapeUriString(uri));
                using (var response = await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = await reader.ReadToEndAsync();
                        try
                        {
                            var data = JArray.Parse(result);
                            return new ApiResponse<T>(new ApiResponseRaw(data));
                        }
                        catch
                        {
                            var data = JObject.Parse(result);
                            return new ApiResponse<T>(new ApiResponseRaw(data));
                        }
                    }
                }
            }
            catch (WebException)
            {
                // No internet connection
                throw new WebException("No internet connection.");
            }
        }

  
        
        #endregion

    }
}