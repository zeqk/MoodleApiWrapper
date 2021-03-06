﻿using Microsoft.Extensions.Options;
using MoodleApiWrapper.Exceptions;
using MoodleApiWrapper.Options;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MoodleApiWrapper
{
    public class MoodleApiClient : IMoodleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<Moodle> _options;

        public MoodleApiClient(HttpClient httpClient, IOptions<Moodle> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

       

   
        #region functions

        #region Helper functions

        private int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return Convert.ToInt32((TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }


        #endregion

        #region Authentications
        /// <summary>
        /// Returns your Api Token needed to make any calls
        /// <para />
        /// service shortname - The service shortname is usually hardcoded in the pre-build service (db/service.php files).
        /// Moodle administrator will be able to edit shortnames for service created on the fly: MDL-29807.
        /// If you want to use the Mobile service, its shortname is moodle_mobile_app. Also useful to know,
        /// the database shortname field can be found in the table named external_services.
        /// </summary>
        /// <param names="username"></param>
        /// <param names="password"></param>
        /// <param names="serviceHostName"></param>
        /// <returns></returns>
        public Task<AuthentiactionResponse<AuthToken>> GetApiToken(string username, string password,
            string serviceHostName)
        {
            var query =
                "login/token.php" +
                $"?username={username}" +
                $"&password={HttpUtility.UrlEncode(password)}" +
                $"&service={serviceHostName}";

            return getAuthAsync<AuthToken>(query);
        }
#endregion

        #region System actions
        /// <summary>
        /// This API will return information about the site, web services users, and authorized API actions. This call is useful for getting site information and the capabilities of the web service user. 
        /// </summary>
        /// <param names="serviceHostNames">Returns information about a particular service.</param>
        /// <returns></returns>
        public Task<ApiResponse<Site_info>> GetSiteInfo(string serviceHostName = "")
        {
            var query = string.Empty;
            if (!serviceHostName.Any())
            {
                query = string.Format("webservice/rest/server.php" +
                                        "?wstoken={0}&moodlewsrestformat={1}&wsfunction={2}",
                    _options.Value.ApiToken, nameof(Format.json), nameof(Methods.core_webservice_get_site_info));
            }
            else
            {
                query = string.Format("webservice/rest/server.php" +
                                        "?wstoken={0}&moodlewsrestformat={1}&wsfunction={2}&serviceshortnames[0]={3}",
                    _options.Value.ApiToken, nameof(Format.json), nameof(Methods.core_webservice_get_site_info),
                    serviceHostName);
            }

            return getAsync<Site_info>(query);
        }
        #endregion

        #region auth_email
        public Task<ApiResponse<Site_info>> GetSignupSettings()
        {
            var query = string.Format("webservice/rest/server.php" +
                                        "?wstoken={0}&moodlewsrestformat={1}&wsfunction={2}",
                    _options.Value.ApiToken, nameof(Format.json), nameof(Methods.auth_email_get_signup_settings));

            return getAsync<Site_info>(query);
        }

        #endregion

        #region User Actions 
        /// <summary>
        /// Search for users matching the parameters of the call. This call will return matching user accounts with profile fields.
        ///  The key/value pairs to be considered in user search. Values can not be empty. Specify different keys only once
        ///  (fullname =&gt; 'user1', auth =&gt; 'manual', ...) - key occurences are forbidden. The search is executed with AND operator on the criterias.
        ///  Invalid criterias (keys) are ignored, the search is still executed on the valid criterias. You can search without criteria,
        ///  but the function is not designed for it. It could very slow or timeout. The function is designed to search some specific users.
        /// <para />
        /// "id" (int) matching user id<para />
        ///"lastname" (string) user last names (Note: you can use % for searching but it may be considerably slower!)<para />
        ///"firstname" (string) user first names (Note: you can use % for searching but it may be considerably slower!)<para />
        ///"idnumber" (string) matching user idnumber<para />
        ///"username" (string) matching user username<para />
        ///"email" (string) user email (Note: you can use % for searching but it may be considerably slower!)<para />
        ///"auth" (string) matching user auth plugin<para />
        /// </summary>
        /// <param names="criteria_key0">Key of the first search parameter.</param>
        /// <param names="criteria_value0">Value of the first search term.</param>
        /// <param names="criteria_key1">Key of the second search parameter.</param>
        /// <param names="criteria_value1">Value of the second search term.</param>
        /// <returns></returns>
        public Task<ApiResponse<Users>> GetUsers(string criteria_key0, string criteria_value0,
            string criteria_key1 = "", string criteria_value1 = "")
        {
            var query = string.Empty;
            if (criteria_key1.Any() && criteria_value1.Any())
            {
                query =
                    "webservice/rest/server.php?" +
                    $"wstoken={_options.Value.ApiToken}&" +
                    $"wsfunction={nameof(Methods.core_user_get_users)}&" +
                    $"moodlewsrestformat={nameof(Format.json)}&" +
                    $"criteria[0][key]={criteria_key0}&" +
                    $"criteria[0][value]={criteria_value0}" +
                    $"criteria[1][key]={criteria_key0}&" +
                    $"criteria[1][value]={criteria_value0}";
            }
            else
            {
                query =
                    "webservice/rest/server.php?" +
                    $"wstoken={_options.Value.ApiToken}&" +
                    $"wsfunction={nameof(Methods.core_user_get_users)}&" +
                    $"moodlewsrestformat={nameof(Format.json)}&" +
                    $"criteria[0][key]={criteria_key0}&" +
                    $"criteria[0][value]={criteria_value0}";
            }
            return getAsync<Users>(query);
        }

        /// <summary>
        /// Retrieve users information for a specified unique field - If you want to do a user search, use GetUsers()
        /// 
        /// Avaiable Criteria:
        ///"id" (int) matching user id
        ///"lastname" (string) user last names (Note: you can use % for searching but it may be considerably slower!)
        ///"firstname" (string) user first names (Note: you can use % for searching but it may be considerably slower!)
        ///"idnumber" (string) matching user idnumber
        ///"username" (string) matching user username
        ///"email" (string) user email (Note: you can use % for searching but it may be considerably slower!)
        ///"auth" (string) matching user auth plugin
        /// </summary>
        /// <param names="criteria_key">Key of the first search parameter.</param>
        /// <param names="criteria_value">Value of the first search term.</param>
        /// <returns></returns>
        public Task<ApiResponse<Users>> GetUsersByField(string criteria_key, string criteria_value)
        {
            var query = string.Empty;
            query =
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_user_get_users_by_field)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"criteria[0][key]={criteria_key}&" +
                $"criteria[0][value]={criteria_value}";

            return getAsync<Users>(query);
        }

        /// <summary>
        /// Get the list of courses where a user is enrolled in 
        /// </summary>
        /// <param names="userid"></param>
        /// <returns></returns>
        public Task<ApiResponse<Cources>> GetUserCourses(int userid)
        {
            var query = string.Empty;
            query =
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_enrol_get_users_courses)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"userid={userid}";

            return getAsync<Cources>(query);
        }

        /// <summary>
        /// Create a User.
        /// </summary>
        /// <param names="username"></param>
        /// <param names="firstname"></param>
        /// <param names="lastname"></param>
        /// <param names="email"></param>
        /// <param names="password"></param>
        /// <param names="auth"></param>
        /// <param names="idnumber"></param>
        /// <param names="lang"></param>
        /// <param names="calendartye"></param>
        /// <param names="theme"></param>
        /// <param names="timezone"></param>
        /// <param names="mailformat"></param>
        /// <param names="description"></param>
        /// <param names="city"></param>
        /// <param names="country"></param>
        /// <param names="firstnamephonetic"></param>
        /// <param names="lastnamephonetic"></param>
        /// <param names="middlename"></param>
        /// <param names="alternatename"></param>
        /// <param names="preferences_type"></param>
        /// <param names="preferences_value"></param>
        /// <param names="customfields_type"></param>
        /// <param names="customfields_value"></param>
        /// <returns></returns>
        public Task<ApiResponse<NewUser>> CreateUser(string username, string firstname, string lastname,
            string email, string password,
            string auth = "", string idnumber = "", string lang = "", string calendartye = "", string theme = "",
            string timezone = "",
            string mailformat = "", string description = "", string city = "", string country = "",
            string firstnamephonetic = "",
            string lastnamephonetic = "", string middlename = "", string alternatename = "",
            string preferences_type = "", string preferences_value = "",
            string customfields_type = "", string customfields_value = "")
        {
            var querybuilder = new StringBuilder();
            querybuilder.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&wsfunction={nameof(Methods.core_user_create_users)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
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


            return getAsync<NewUser>(querybuilder.ToString());
        }


        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param names="id"></param>
        /// <param names="username"></param>
        /// <param names="firstname"></param>
        /// <param names="lastname"></param>
        /// <param names="email"></param>
        /// <param names="password"></param>
        /// <param names="auth"></param>
        /// <param names="idnumber"></param>
        /// <param names="lang"></param>
        /// <param names="calendartye"></param>
        /// <param names="theme"></param>
        /// <param names="timezone"></param>
        /// <param names="mailformat"></param>
        /// <param names="description"></param>
        /// <param names="city"></param>
        /// <param names="country"></param>
        /// <param names="firstnamephonetic"></param>
        /// <param names="lastnamephonetic"></param>
        /// <param names="middlename"></param>
        /// <param names="alternatename"></param>
        /// <param names="preferences_type"></param>
        /// <param names="preferences_value"></param>
        /// <param names="customfields_type"></param>
        /// <param names="customfields_value"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> UpdateUser(int id, string username = "", string firstname = "",
            string lastname = "",
            string email = "", string password = "", string auth = "", string idnumber = "", string lang = "",
            string calendartye = "", string theme = "",
            string timezone = "", string mailformat = "", string description = "", string city = "", string country = "",
            string firstnamephonetic = "",
            string lastnamephonetic = "", string middlename = "", string alternatename = "",
            string preferences_type = "", string preferences_value = "",
            string customfields_type = "", string customfields_value = "")
        {
            var querybuilder = new StringBuilder();
            querybuilder.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&wsfunction={nameof(Methods.core_user_update_users)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
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


            return getAsync<Success>(querybuilder.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param names="id"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> DeleteUser(int id)
        {
            var query = string.Empty;
            query =
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_user_delete_users)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"userids[0]={id}";

            return getAsync<Success>(query);

        }

        /// <summary>
        /// Manual role assignments. This call should be made in an array. 
        /// </summary>
        /// <param names="role_id">
        /// <summary>Role to assign to the user</summary>
        /// </param>
        /// <param names="user_id"></param>
        /// <param names="context_id"></param>
        /// <param names="context_level"></param>
        /// <param names="instance_id"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> AssignRoles(int role_id, int user_id, string context_id = "",
            string context_level = "", int instance_id = Int32.MinValue)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_role_assign_roles)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"assignments[0][roleid]={role_id}&" +
                $"assignments[0][userid]={user_id}");
            if (context_id.Any()) query.Append($"&assignments[0][contextid]={context_id}");
            if (context_level.Any()) query.Append($"&assignments[0][contextlevel]={context_level}");
            if (instance_id != Int32.MinValue) query.Append($"&assignments[0][instanceid]={instance_id}");

            return getAsync<Success>(query.ToString());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param names="role_id"></param>
        /// <param names="user_id"></param>
        /// <param names="context_id"></param>
        /// <param names="context_level"></param>
        /// <param names="instance_id"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> UnassignRoles(int role_id, int user_id, string context_id = "",
           string context_level = "", int instance_id = Int32.MinValue)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_role_unassign_roles)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"unassignments[0][roleid]={role_id}&" +
                $"unassignments[0][userid]={user_id}");
            if (context_id.Any()) query.Append($"&unassignments[0][contextid]={context_id}");
            if (context_level.Any()) query.Append($"&unassignments[0][contextlevel]={context_level}");
            if (instance_id != Int32.MinValue) query.Append($"&unassignments[0][instanceid]={instance_id}");

            return getAsync<Success>(query.ToString());
        }

        #endregion

        #region Course Enrollment Actions
        /// <summary>
        /// 
        /// </summary>
        /// <param names="role_id"></param>
        /// <param names="user_id"></param>
        /// <param names="cource_id"></param>
        /// <param names="timestart"></param>
        /// <param names="timeend"></param>
        /// <param names="suspend"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> EnrolUser(int role_id, int user_id, int cource_id,
                                                            int timestart = Int32.MinValue, int timeend = Int32.MinValue, int suspend = Int32.MinValue)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.enrol_manual_enrol_users)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"enrolments[0][roleid]={role_id}&" +
                $"enrolments[0][userid]={user_id}&" + 
                $"enrolments[0][courceid]={cource_id}");
            if (timestart != Int32.MinValue) query.Append($"&enrolments[0][timestart]={timestart}");
            if (timeend != Int32.MinValue) query.Append($"&enrolments[0][timeend]={timeend}");
            if (suspend != Int32.MinValue) query.Append($"&enrolments[0][suspend]={suspend}");

            return getAsync<Success>(query.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param names="group_id"></param>
        /// <param names="user_id"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> AddGroupMember(int group_id, int user_id)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_group_add_group_members)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"members[0][groupid]={group_id}&" +
                $"members[0][userid]={user_id}");               

            return getAsync<Success>(query.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param names="group_id"></param>
        /// <param names="user_id"></param>
        /// <returns></returns>
        public Task<ApiResponse<Success>> DeleteGroupMember(int group_id, int user_id)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_group_delete_group_members)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"members[0][groupid]={group_id}&" +
                $"members[0][userid]={user_id}");

            return getAsync<Success>(query.ToString());
        }


        #endregion

        #region Course Actions
       
        /// <summary>
        /// Get a listing of categories in the system. 
        /// </summary>
        /// <param names="criteria_key">
        /// <summary>
        /// criteria[0][key] - The category column to search, expected keys (value format) are:"id" (int) the category id,"names" (string)
        ///  the category names,"parent" (int) the parent category id,"idnumber" (string) category idnumber - user must have 'moodle/category:manage'
        ///  to search on idnumber,"visible" (int) whether the returned categories must be visible or hidden.
        ///  If the key is not passed, then the function return all categories that the user can see. - user must have 'moodle/category:manage'
        ///  or 'moodle/category:viewhiddencategories' to search on visible,"theme" (string) only return the categories having this theme
        ///  - user must have 'moodle/category:manage' to search on theme
        /// </summary>
        /// </param>
        /// <param names="criteria_value"><summary>Criteria[0][value] - The value to match</summary></param>
        /// <param names="addSubCategories"><summary>Return the sub categories infos (1 - default) otherwise only the category info (0)</summary></param>
        /// <returns></returns>
        public Task<ApiResponse<Category>> GetCategories(string criteria_key, string criteria_value, int addSubCategories = 1 )
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_course_get_categories)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"criteria[0][key]={criteria_key}&" +
                $"criteria[0][value]={criteria_value}");

            if (addSubCategories != 1) query.Append($"&addsubcategories={addSubCategories}");

            return getAsync<Category>(query.ToString());
        }

        /// <summary>
        /// Get a listing of courses in the system. 
        /// </summary>
        /// <param names="options"><summary>List of course id.If empty return all courses except front page course.</summary></param>
        /// <returns></returns>
        public Task<ApiResponse<Course>> GetCourses(int options = int.MinValue)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_course_get_courses)}&" +
                $"moodlewsrestformat={nameof(Format.json)}"); 
                if (options != int.MinValue) query.Append($"&addsubcategories={options}");

            return getAsync<Course>(query.ToString());
        }

        /// <summary>
        /// Get course contents
        /// </summary>
        /// <param names="course_id"><summary>Course Id</summary></param>
        /// <returns></returns>
        public Task<ApiResponse<Content>> GetContents(int course_id)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_course_get_contents)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&"+
                $"courseid={course_id}");
                
            return getAsync<Content>(query.ToString());
        }

        /// <summary>
        /// Returns group details. 
        /// </summary>
        /// <param names="group_id">Group id</param>
        /// <returns></returns>
        public Task<ApiResponse<Group>> GetGroup(int group_id)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_group_get_groups)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"groupids[0]={group_id}");

            return getAsync<Group>(query.ToString());
        }
        /// <summary>
        /// Returns group details. 
        /// </summary>
        /// <param names="group_ids"><summary>Group Ids</summary></param>
        /// <returns></returns>
        public Task<ApiResponse<Group>> GetGroups(int[] group_ids)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_group_get_groups)}&" +
                $"moodlewsrestformat={nameof(Format.json)}");

            for (int i = 0; i < group_ids.Count(); i++)
            {
                query.Append($"&groupids[{i}]={group_ids[i]}");
            }

            return getAsync<Group>(query.ToString());
        }

        /// <summary>
        /// Returns all groups in specified course. 
        /// </summary>
        /// <param names="course_id"><summary>Course Id</summary></param>
        /// <returns></returns>
        public Task<ApiResponse<Group>> GetCourseGroups(int course_id)
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_group_get_course_groups)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&"+
                $"courseid={course_id}");

            return getAsync<Group>(query.ToString());
        }

        /// <summary>
        /// Get enrolled users by course id. 
        /// </summary>
        /// <param names="course_id"></param>
        /// <returns></returns>
        public Task<ApiResponse<EnrolledUser>> GetEnrolledUsersByCourse(int course_id)
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_enrol_get_enrolled_users)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"courseid={course_id}");

            return getAsync<EnrolledUser>(query.ToString());
        }

        /// <summary>
        /// Create new course
        /// </summary>
        /// <param names="fullname"><summary>Full names of the course</summary></param>
        /// <param names="shortname"><summary>Shortname of the course</summary></param>
        /// <param names="category_id"><summary>Category ID of the course</summary></param>
        /// <param names="idnumber"><summary>Optional //id number</summary></param>
        /// <param names="summary"><summary>Optional //summary</summary></param>
        /// <param names="summaryformat"><summary>Default to "1" //summary format (1 = HTML, 0 = MOODLE, 2 = PLAIN or 4 = MARKDOWN)</summary></param>
        /// <param names="format"><summary>Default to "topics" //course format: weeks, topics, social, site,..</summary></param>
        /// <param names="showgrades"><summary>Default to "0" //1 if grades are shown, otherwise 0</summary></param>
        /// <param names="newsitems"><summary>Default to "0" //number of recent items appearing on the course page</summary></param>
        /// <param names="startdate"><summary>Optional //timestamp when the course start</summary></param>
        /// <param names="numsections"><summary>Optional //(deprecated, use courseformatoptions) number of weeks/topics</summary></param>
        /// <param names="maxbytes"><summary>Default to "104857600" //largest size of file that can be uploaded into the course</summary></param>
        /// <param names="showreports"><summary>Default to "1" //are activity report shown (yes = 1, no =0)</summary></param>
        /// <param names="visible"><summary>Optional //1: available to student, 0:not available</summary></param>
        /// <param names="hiddensections"><summary>Optional //(deprecated, use courseformatoptions) How the hidden sections in the course are displayed to students</summary></param>
        /// <param names="groupmode"><summary>Default to "0" //no group, separate, visible</summary></param>
        /// <param names="groupmodeforce"><summary>Default to "0" //1: yes, 0: no</summary></param>
        /// <param names="defaultgroupingid"><summary>Default to "0" //default grouping id</summary></param>
        /// <param names="enablecompletion"><summary>Optional //Enabled, control via completion and activity settings. Disabled, not shown in activity settings.</summary></param>
        /// <param names="completenotify"><summary>Optional //1: yes 0: no</summary></param>
        /// <param names="lang"><summary>//forced course language</summary></param>
        /// <param names="forcetheme"><summary>Optional //names of the force theme</summary></param>
        /// <param names="courcCourseformatoption"><summary>Optional //additional options for particular course format list of ( object { names string //course format option names
        ///value string //course format option value } )} )</summary></param>
        /// <returns></returns>
        public Task<ApiResponse<NewCourse>> CreateCourse(string fullname, string shortname, int category_id,
            string idnumber = "", string summary = "", int summaryformat = 1, string format = "", int showgrades = 0, int newsitems = 0,
            DateTime startdate = default(DateTime), int numsections = int.MaxValue, int maxbytes = 104857600, int showreports = 1, 
            int visible = 0, int hiddensections = int.MaxValue, int groupmode = 0,
            int groupmodeforce = 0, int defaultgroupingid = 0, int enablecompletion = int.MaxValue,
            int completenotify = 0, string lang = "", string forcetheme = "",
            string courcCourseformatoption = ""/*not implemented*/)
        {
            
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_course_create_courses)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"courses[0][fullname]={fullname}&"+
                $"courses[0][shortname]={shortname}&"+
                $"courses[0][categoryid]={category_id}");

            if(idnumber.Any()) query.Append($"&courses[0][idnumber]={idnumber}");
            if (summary.Any()) query.Append($"&courses[0][summary]={summary}");
            if (summaryformat != 1) query.Append($"&courses[0][summaryformat ]={summaryformat}");
            if (format.Any()) query.Append($"&courses[0][format]={format}");
            if (showgrades != 0) query.Append($"&courses[0][showgrades]={showgrades}");
            if (!startdate.Equals(default(DateTime))) query.Append($"&courses[0][startdate]={DateTimeToUnixTimestamp(startdate)}");
            if (newsitems!=0) query.Append($"&courses[0][newsitems]={newsitems}");
            if (numsections != int.MaxValue) query.Append($"&courses[0][numsections]={numsections}");
            if (maxbytes != 104857600) query.Append($"&courses[0][maxbytes]={category_id}");
            if (showreports != 1) query.Append($"&courses[0][showreports]={showreports}");
            if (visible != 0) query.Append($"&courses[0][visible]={visible}");
            if (hiddensections != int.MaxValue) query.Append($"&courses[0][hiddensections]={hiddensections}");
            if (groupmode != 0) query.Append($"&courses[0][groupmode]={groupmode}");
            if (groupmodeforce != 0) query.Append($"&courses[0][groupmodeforce]={groupmodeforce}");
            if (defaultgroupingid != 0) query.Append($"&courses[0][defaultgroupingid]={defaultgroupingid}");
            if (enablecompletion != int.MaxValue) query.Append($"&courses[0][enablecompletion]={enablecompletion}");
            if (completenotify != 0) query.Append($"&courses[0][completenotify]={completenotify}");
            if (lang.Any()) query.Append($"&courses[0][lang]={lang}");
            if (forcetheme.Any()) query.Append($"&courses[0][forcetheme]={forcetheme}");

            return getAsync<NewCourse>(query.ToString());
        }
        /// <summary>
        /// Create new courses
        /// </summary>
        /// <param names="fullname"></param>
        /// <param names="shortname"></param>
        /// <param names="category_ids"></param>
        /// <returns></returns>
        public Task<ApiResponse<NewCourse>> CreateCourses(string[] fullname, string[] shortname,int[] category_ids)
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_course_create_courses)}&" +
                $"moodlewsrestformat={nameof(Format.json)}");
            for (int i = 0; i < fullname.Count(); i++)
            {
                query.Append(
                    $"&courses[{i}][fullname]={fullname[i]}" +
                    $"&courses[{i}][shortname]={shortname[i]}" +
                    $"&courses[{i}][categoryid]={category_ids[i]}");
            }

            return getAsync<NewCourse>(query.ToString());
        }

        public Task<ApiResponse<UpdateCourseRoot>> UpdateCourse(int id, string fullname = "", string shortname = "", int category_id = Int32.MaxValue,
    string idnumber = "", string summary = "", int summaryformat = 1, string format = "", int showgrades = 0, int newsitems = 0,
    DateTime startdate = default(DateTime), int numsections = int.MaxValue, int maxbytes = 104857600, int showreports = 1,
    int visible = 0, int hiddensections = int.MaxValue, int groupmode = 0,
    int groupmodeforce = 0, int defaultgroupingid = 0, int enablecompletion = int.MaxValue,
    int completenotify = 0, string lang = "", string forcetheme = "",
    string courcCourseformatoption = ""/*not implemented*/)
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_course_update_courses)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"courses[0][id]={id}");

            if (fullname.Any()) query.Append($"&courses[0][fullname]={fullname}");
            if (shortname.Any()) query.Append($"&courses[0][shortname]={shortname}");
            if (category_id != Int32.MaxValue) query.Append($"&courses[0][categoryid]={category_id}");
            if (idnumber.Any()) query.Append($"&courses[0][idnumber]={idnumber}");
            if (summary.Any()) query.Append($"&courses[0][summary]={summary}");
            if (summaryformat != 1) query.Append($"&courses[0][summaryformat ]={summaryformat}");
            if (format.Any()) query.Append($"&courses[0][format]={format}");
            if (showgrades != 0) query.Append($"&courses[0][showgrades]={showgrades}");
            if (!startdate.Equals(default(DateTime))) query.Append($"&courses[0][startdate]={DateTimeToUnixTimestamp(startdate)}");
            if (newsitems != 0) query.Append($"&courses[0][newsitems]={newsitems}");
            if (numsections != int.MaxValue) query.Append($"&courses[0][numsections]={numsections}");
            if (maxbytes != 104857600) query.Append($"&courses[0][maxbytes]={category_id}");
            if (showreports != 1) query.Append($"&courses[0][showreports]={showreports}");
            if (visible != 0) query.Append($"&courses[0][visible]={visible}");
            if (hiddensections != int.MaxValue) query.Append($"&courses[0][hiddensections]={hiddensections}");
            if (groupmode != 0) query.Append($"&courses[0][groupmode]={groupmode}");
            if (groupmodeforce != 0) query.Append($"&courses[0][groupmodeforce]={groupmodeforce}");
            if (defaultgroupingid != 0) query.Append($"&courses[0][defaultgroupingid]={defaultgroupingid}");
            if (enablecompletion != int.MaxValue) query.Append($"&courses[0][enablecompletion]={enablecompletion}");
            if (completenotify != 0) query.Append($"&courses[0][completenotify]={completenotify}");
            if (lang.Any()) query.Append($"&courses[0][lang]={lang}");
            if (forcetheme.Any()) query.Append($"&courses[0][forcetheme]={forcetheme}");

            return getAsync<UpdateCourseRoot>(query.ToString());
        }

        #endregion

        #region Grade Actions

        /// <summary>
        /// Returns grade item details and optionally student grades. 
        /// </summary>
        /// <param names="criteria_key"></param>
        /// <param names="criteria_value"></param>
        /// <param names="addSubCategories"></param>
        /// <returns></returns>
        public Task<ApiResponse<Category>> GetGrades(int courseid, string component = "", int activityid = Int32.MaxValue, string[] userids = null)
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_grades_get_grades)}&" +
                $"moodlewsrestformat={nameof(Format.json)}&" +
                $"courseid={courseid}");

            if (component.Any()) query.Append($"&component={component}");
            if (activityid != int.MaxValue) query.Append($"&activityid={activityid}");
            if (userids != null) query.Append($"&userids={userids}");
            if (component.Any()) query.Append($"&component={component}");

            return getAsync<Category>(query.ToString());
        }



        #endregion

        #region Calander Actions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupids"></param>
        /// <param name="courseids"></param>
        /// <param name="eventids"></param>
        /// <returns></returns>
        public Task<ApiResponse<Events>> GetCalanderEvents(int[] groupids = default(int[]), int[] courseids = default(int[]),int[] eventids = default(int[]))
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_calendar_get_calendar_events)}&" +
                $"moodlewsrestformat={nameof(Format.json)}");

            if (groupids != null)
                for (int i = 0; i < groupids.Count(); i++)                
                    query.Append($"&events[groupids][{i}]={groupids[i]}");

            if (courseids != null)
                for (int i = 0; i < courseids.Count(); i++)                
                    query.Append($"&events[courseids][{i}]={courseids[i]}");
                
            if(eventids != null)
                for (int i = 0; i < eventids.Count(); i++)
                    query.Append($"&events[eventids][{i}]={eventids[i]}");
                
            return getAsync<Events>(query.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="names"></param>
        /// <param name="descriptions"></param>
        /// <param name="formats"></param>
        /// <param name="groupids"></param>
        /// <param name="courseids"></param>
        /// <param name="repeats"></param>
        /// <param name="eventtypes"></param>
        /// <param name="timestarts"></param>
        /// <param name="timedurations"></param>
        /// <param name="visible"></param>
        /// <param name="sequences"></param>
        /// <returns></returns>
        public Task<ApiResponse<Events>> CreateCalanderEvents(string[] names, string[] descriptions = default(string[]),
             int[] formats = default(int[]), int[] groupids = default(int[]), int[] courseids = default(int[]), int[] repeats = default(int[]),
             string[] eventtypes = default(string[]), DateTime[] timestarts = default(DateTime[]), TimeSpan[] timedurations = default(TimeSpan[]),
             int[] visible = default(int[]), int[] sequences = default(int[]))
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_calendar_create_calendar_events)}&" +
                $"moodlewsrestformat={nameof(Format.json)}");

            for (int i = 0; i < names.Count(); i++)
                query.Append($"&events[{i}][name]={names[i]}");

            if (groupids != null)
                for (int i = 0; i < groupids.Count(); i++)
                    query.Append($"&events[{i}][groupid]={groupids[i]}");

            if (courseids != null)
                for (int i = 0; i < courseids.Count(); i++)
                    query.Append($"&events[{i}][courseid]={courseids[i]}");

            if (descriptions != null)
                for (int i = 0; i < descriptions.Count(); i++)
                    query.Append($"&events[{i}][description]={descriptions[i]}");

            if (formats != null)
                for (int i = 0; i < formats.Count(); i++)
                    query.Append($"&events[{i}][format]={formats[i]}");

            if (repeats != null)
                for (int i = 0; i < repeats.Count(); i++)
                    query.Append($"&events[{i}][repeats]={repeats[i]}");

            if (timestarts != null)
                for (int i = 0; i < timestarts.Count(); i++)
                    query.Append($"&events[{i}][timestart]={DateTimeToUnixTimestamp(timestarts[i])}");

            if (timedurations != null)
                for (int i = 0; i < timedurations.Count(); i++)
                    query.Append($"&events[{i}][timeduration]={timedurations[i].TotalSeconds}");

            if (visible != null)
                for (int i = 0; i < visible.Count(); i++)
                    query.Append($"&events[{i}][visible]={visible[i]}");

            if (sequences != null)
                for (int i = 0; i < sequences.Count(); i++)
                    query.Append($"&events[{i}][sequence]={sequences[i]}");

            return getAsync<Events>(query.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="repeats"></param>
        /// <param name="descriptions"></param>
        /// <returns></returns>
        public Task<ApiResponse<Events>> DeleteCalanderEvents(int[] eventids,int[] repeats, string[] descriptions = default(string[]))
        {
            StringBuilder query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={nameof(Methods.core_calendar_delete_calendar_events)}&" +
                $"moodlewsrestformat={nameof(Format.json)}");

            if (repeats != null)
                for (int i = 0; i < repeats.Count(); i++)
                    query.Append($"&events[{i}][repeat]={repeats[i]}");

            if (eventids != null)
                for (int i = 0; i < eventids.Count(); i++)
                    query.Append($"&events[{i}][eventid]={eventids[i]}");


            if (descriptions != null)
                for (int i = 0; i < descriptions.Count(); i++)
                    query.Append($"&events[{i}][description]={descriptions[i]}");
                
            return getAsync<Events>(query.ToString());
        }

        #endregion

        #region Group Actions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="names"></param>
        /// <param name="courseids"></param>
        /// <param name="descriptions"></param>
        /// <param name="descriptionformats"></param>
        /// <param name="enrolmentkeys"></param>
        /// <param name="idnumbers"></param>
        /// <returns></returns>
        public Task<ApiResponse<Group>> CreateGroups(string[] names = default(string[]), int[] courseids = default(int[]), string[] descriptions = default(string[]),
            int[] descriptionformats = default (int[]), string[] enrolmentkeys = default(string[]),string[] idnumbers = default (string[]))
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction=core_group_create_groups&" +
                $"moodlewsrestformat={nameof(Format.json)}");

            if (names != null)
                for (int i = 0; i < names.Count(); i++)
                    query.Append($"&groups[{i}][name]={names[i]}");

            if (courseids != null)
                for (int i = 0; i < courseids.Count(); i++)
                    query.Append($"&groups[{i}][courseid]={courseids[i]}");

            if (descriptions != null)
                for (int i = 0; i < descriptions.Count(); i++)
                    query.Append($"&groups[{i}][description]={descriptions[i]}");

            if (descriptionformats != null)
                for (int i = 0; i < descriptionformats.Count(); i++)
                    query.Append($"&groups[{i}][descriptionformat]={descriptionformats[i]}");

            if (enrolmentkeys != null)
                for (int i = 0; i < enrolmentkeys.Count(); i++)
                    query.Append($"&groups[{i}][enrolmentkey]={enrolmentkeys[i]}");

            if (idnumbers != null)
                for (int i = 0; i < idnumbers.Count(); i++)
                    query.Append($"&groups[{i}][idnumber]={idnumbers[i]}");


            return getAsync<Group>(query.ToString());
        }
        #endregion


       
        public async Task<ApiResponse<T>> ExecuteMethod<T>(string methodName, object parameter) where T : IDataModel
        {
            var query = new StringBuilder();
            query.Append(
                "webservice/rest/server.php?" +
                $"wstoken={_options.Value.ApiToken}&" +
                $"wsfunction={methodName}&" +
                $"moodlewsrestformat={nameof(Format.json)}");
            if (parameter != null)
            {
                query.Append("&");
                query.Append(parameter.ToQueryString());
            }
            var apiResponse = await  getAsync<T>(query.ToString());
            if(apiResponse.Status == "Failed")
            {
                switch (apiResponse.Error.errorcode)
                {
                    case "invalidparameter":
                        throw new InvalidParameterException(apiResponse.ResponseText, apiResponse.RequestedPath, apiResponse.Error);
                    default:
                        break;
                }
            }
            return apiResponse;
        }


        #region Getters
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam names="T"></typeparam>
        /// <param names="uri"></param>
        /// <returns></returns>
        private async Task<AuthentiactionResponse<T>> getAuthAsync<T>(string path) where T : IDataModel
        {   
            var response = await _httpClient.GetAsync(path);
            var result = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(result);
            return new AuthentiactionResponse<T>(new AuthentiactionResponseRaw(data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam names="T"></typeparam>
        /// <param names="uri"></param>
        /// <returns></returns>
        private async Task<ApiResponse<T>> getAsync<T>(string path) where T : IDataModel
        {
            var response =  await _httpClient.GetAsync(path);
            var result = await response.Content.ReadAsStringAsync();

            ApiResponse<T> rv = null;
            if (result.ToLower() == "null")
                result = "{IsSuccessful: true,}";
            try
            {
                var data = JArray.Parse(result);
                rv = new ApiResponse<T>(new ApiResponseRaw(data));
            }
            catch (Exception ex)
            {
                var data = JObject.Parse(result);
                rv = new ApiResponse<T>(new ApiResponseRaw(data));
            }
            rv.RequestedPath = path;
            rv.ResponseText = result;
            return rv;
        }
        #endregion
        
        #endregion
    }
}

