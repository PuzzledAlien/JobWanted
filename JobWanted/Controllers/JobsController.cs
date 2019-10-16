﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System;
using System.Net.Http.Headers;
using AngleSharp.Html.Parser;
using Talk.Cache;
using Newtonsoft.Json;
using JobWanted.Dto;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
/*
 需要安装：
 1、AngleSharp 【html解析组件】
 2、System.Text.Encoding.CodePages 【Encoding.GetEncoding("GBK")编码提供程序】
*/

namespace JobWanted.Controllers
{
    [Route("api/[controller]/[action]")]
    public class JobsController : Controller
    {
        /// <summary>
        /// 获取智联信息(简要信息)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<JobInfo>> GetJobsByZL(string city, string key, int index)
        {
            var cache = GetCacheObject();
            var data = cache.GetData();
            if (data != null)
                return data.Data;

            var cityCode = CodesData.GetCityCode(RecruitEnum.智联招聘, city);
            const int pageSize = 90;
            var start = (index - 1) * pageSize;
            var url = $"https://fe-api.zhaopin.com/c/i/sou?start={start}&pageSize={pageSize}&cityId={cityCode}&salary=0,0&workExperience=-1&education=-1&companyType=-1&employmentType=-1&jobWelfareTag=-1&sortType=publish&kw={key}&kt=3";
            using var http = new HttpClient();
            var htmlString = await http.GetStringAsync(url);
            var zlResponse = JsonConvert.DeserializeObject<ZlResponse<ZlSouResponse>>(htmlString);
            var jobInfos = zlResponse?.Data?.Results?.Select(t => new JobInfo
            {
                PositionName = t.JobName,
                CorporateName = t.Company.Name,
                Salary = t.Salary,
                WorkingPlace = $"{t.City.Display}{(string.IsNullOrEmpty(t.BusinessArea) ? string.Empty : "-" + t.BusinessArea)}",
                ReleaseDate = t.UpdateDate,
                DetailsUrl = t.PositionURL,
            }).ToList();
            cache.AddData(jobInfos);
            return jobInfos;
        }

        /// <summary>
        /// 获取猎聘信息(简要信息)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<JobInfo>> GetJobsByLP(string city, string key, int index)
        {
            var cache = GetCacheObject();
            var data = cache.GetData();
            if (data != null)
                return data.Data;

            var cityCode = CodesData.GetCityCode(RecruitEnum.猎聘网, city);
            var url = $"https://www.liepin.cn/zhaopin/?key={key}&dqs={cityCode}&curPage={index}";
            using var http = new HttpClient();
            var htmlString = await http.GetStringAsync(url);
            var htmlParser = new HtmlParser();
            var document = await htmlParser.ParseDocumentAsync(htmlString);
            var jobInfos = document.QuerySelectorAll("ul.sojob-list li")
                .Where(t => t.QuerySelectorAll(".job-info h3 a").FirstOrDefault() != null)
                .Select(t => new JobInfo()
                {
                    PositionName = t.QuerySelectorAll(".job-info h3 a").FirstOrDefault().TextContent,
                    CorporateName = t.QuerySelectorAll(".company-name a").FirstOrDefault().TextContent,
                    Salary = t.QuerySelectorAll(".text-warning").FirstOrDefault().TextContent,
                    WorkingPlace = t.QuerySelectorAll(".area").FirstOrDefault().TextContent,
                    ReleaseDate = t.QuerySelectorAll(".time-info time").FirstOrDefault().TextContent,
                    DetailsUrl = t.QuerySelectorAll(".job-info h3 a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                })
                .ToList();

            cache.AddData(jobInfos);
            return jobInfos;
        }

        /// <summary>
        /// 获取前程信息(简要信息)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<JobInfo>> GetJobsByQC(string city, string key, int index)
        {
            var cache = GetCacheObject();
            var data = cache.GetData();
            if (data != null)
                return data.Data;

            var cityCode = CodesData.GetCityCode(RecruitEnum.前程无忧, city);
            var url = $"http://search.51job.com/jobsearch/search_result.php?jobarea={cityCode}&keyword={key}&curr_page={index}";
            using var http = new HttpClient();
            var htmlBytes = await http.GetByteArrayAsync(url);
            //【注意】使用GBK需要 Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册编码提供程序
            var htmlString = Encoding.GetEncoding("GBK").GetString(htmlBytes);
            var htmlParser = new HtmlParser();
            var document = await htmlParser.ParseDocumentAsync(htmlString);
            var jobInfos = document.QuerySelectorAll(".dw_table div.el")
                .Where(t => t.QuerySelectorAll(".t1 span a").FirstOrDefault() != null)
                .Select(t => new JobInfo()
                {
                    PositionName = t.QuerySelectorAll(".t1 span a").FirstOrDefault().TextContent,
                    CorporateName = t.QuerySelectorAll(".t2 a").FirstOrDefault().TextContent,
                    Salary = t.QuerySelectorAll(".t3").FirstOrDefault().TextContent,
                    WorkingPlace = t.QuerySelectorAll(".t4").FirstOrDefault().TextContent,
                    ReleaseDate = t.QuerySelectorAll(".t5").FirstOrDefault().TextContent,
                    DetailsUrl = t.QuerySelectorAll(".t1 span a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                })
                .ToList();
            cache.AddData(jobInfos);
            return jobInfos;
        }

        /// <summary>
        /// BOSS直聘信息(简要信息)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<JobInfo>> GetJobsByBS(string city, string key, int index)
        {
            if (index <= 0)
            {
                index = 1;
            }
            var cache = GetCacheObject(20);
            var data = cache.GetData();
            if (data != null)
                return data.Data;

            var cityCode = CodesData.GetCityCode(RecruitEnum.BOSS, city);
            var url = $"https://www.zhipin.com/c{cityCode}/?query={key}&page={index}&ka=page-{index}";
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = true
            };
            using var http = new HttpClient(handler);
            var htmlString = await http.GetStringAsync(url);
            var htmlParser = new HtmlParser();
            var document = await htmlParser.ParseDocumentAsync(htmlString);
            var jobInfos = document.QuerySelectorAll(".job-list ul li")
                .Where(t => t.QuerySelectorAll(".info-primary h3").FirstOrDefault() != null)
                .Select(t => new JobInfo()
                {
                    PositionName = t.QuerySelectorAll(".info-primary h3").FirstOrDefault().TextContent,
                    CorporateName = t.QuerySelectorAll(".company-text h3").FirstOrDefault().TextContent,
                    Salary = t.QuerySelectorAll(".info-primary h3 .red").FirstOrDefault().TextContent,
                    WorkingPlace = t.QuerySelectorAll(".info-primary p").FirstOrDefault().TextContent,
                    ReleaseDate = t.QuerySelectorAll(".job-time .time").FirstOrDefault().TextContent,
                    DetailsUrl = "http://www.zhipin.com" + t.QuerySelectorAll("a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                })
                .ToList();

            cache.AddData(jobInfos);//添加缓存
            return jobInfos;
        }

        /// <summary>
        /// 获取拉勾信息(简要信息)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<JobInfo>> GetJobsByLG(string city, string key, int index)
        {
            var cache = GetCacheObject(20);
            var data = cache.GetData();
            if (data != null)
                return data.Data;

            var searchUrl = $@"https://www.lagou.com/jobs/list_{key}?px=new&city={city}#order";
            var option = new ChromeOptions();
            option.AddArgument("headless");
            using var driver = new ChromeDriver(option);
            var navigate = driver.Navigate();
            navigate.GoToUrl(searchUrl);
            var pageSource = driver.PageSource;
            var htmlParser = new HtmlParser();
            var document = await htmlParser.ParseDocumentAsync(pageSource);
            var elements = document.QuerySelectorAll(".s_position_list ul li");
            var jobInfos = new List<JobInfo>();
            foreach (var element in elements)
            {
                if (!element.ClassName.Contains("con_list_item"))
                {
                    continue;
                }
                var positionName = element.QuerySelector(".position_link h3").TextContent;
                var corporateName = element.QuerySelector(".company_name a").TextContent;
                var salary = element.QuerySelector(".p_bot div span").TextContent;
                var workingPlace = element.QuerySelector(".position_link span em").TextContent;
                var releaseDate = element.QuerySelector(".format-time").TextContent;
                var detailsUrl = element.QuerySelector(".position_link").GetAttribute("href");

                var jobInfo = new JobInfo
                {
                    PositionName = positionName,
                    CorporateName = corporateName,
                    DetailsUrl = detailsUrl,
                    WorkingPlace = workingPlace,
                    Salary = salary,
                    ReleaseDate = releaseDate
                };

                jobInfos.Add(jobInfo);
            }

            cache.AddData(jobInfos);//添加缓存
            return jobInfos;
        }

        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<DetailsInfo> GetDetailsInfoByLP(string url)
        {
            using var http = new HttpClient();
            var htmlString = await http.GetStringAsync(url);
            var htmlParser = new HtmlParser();
            var detailsInfo = htmlParser.ParseDocument(htmlString)
                .QuerySelectorAll(".wrap")
                .Where(t => t.QuerySelectorAll(".job-qualifications").FirstOrDefault() != null)
                .Select(t => new DetailsInfo()
                {
                    Experience = t.QuerySelectorAll(".job-qualifications span")[1].TextContent,
                    Education = t.QuerySelectorAll(".job-qualifications span")[0].TextContent,
                    CompanyNature = t.QuerySelectorAll(".new-compintro li")[0].TextContent,
                    CompanySize = t.QuerySelectorAll(".new-compintro li")[1].TextContent,
                    Requirement = t.QuerySelectorAll(".job-item.main-message").FirstOrDefault().TextContent.Replace("职位描述：", ""),
                    CompanyIntroduction = t.QuerySelectorAll(".job-item.main-message.noborder").FirstOrDefault().TextContent,
                })
                .FirstOrDefault();
            return detailsInfo;
        }

        /// <summary>
        /// 获取智联详细信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<DetailsInfo> GetDetailsInfoByZL(string url)
        {
            using HttpClient http = new HttpClient();
            var htmlString = await http.GetStringAsync(url);
            HtmlParser htmlParser = new HtmlParser();
            var detailsInfo = htmlParser.ParseDocument(htmlString)
                .QuerySelectorAll(".terminalpage")
                .Where(t => t.QuerySelectorAll(".terminalpage-left .terminal-ul li").FirstOrDefault() != null)
                .Select(t => new DetailsInfo()
                {
                    Experience = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[4].TextContent,
                    Education = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[5].TextContent,
                    CompanyNature = t.QuerySelectorAll(".terminalpage-right .terminal-company li")[1].TextContent,
                    CompanySize = t.QuerySelectorAll(".terminalpage-right .terminal-company li")[0].TextContent,
                    Requirement = t.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[0].TextContent.Replace("职位描述：", ""),
                    CompanyIntroduction = t.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[1].TextContent,
                })
                .FirstOrDefault();
            return detailsInfo;
        }

        /// <summary>
        /// 获取boss详细信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<DetailsInfo> GetDetailsInfoByBS(string url)
        {
            using HttpClient http = new HttpClient();
            var htmlString = await http.GetStringAsync(url);
            HtmlParser htmlParser = new HtmlParser();
            var detailsInfo = htmlParser.ParseDocument(htmlString)
                .QuerySelectorAll("#main")
                .Where(t => t.QuerySelectorAll(".job-banner .info-primary p").FirstOrDefault() != null)
                .Select(t => new DetailsInfo()
                {
                    Experience = t.QuerySelectorAll(".job-banner .info-primary p").FirstOrDefault().TextContent,
                    //Education = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[5].TextContent,
                    CompanyNature = t.QuerySelectorAll(".job-banner .info-company p").FirstOrDefault().TextContent,
                    //CompanySize = t.QuerySelectorAll(".terminalpage-right .terminal-company li")[0].TextContent,
                    Requirement = t.QuerySelectorAll(".detail-content div.text").FirstOrDefault().TextContent.Replace("职位描述：", ""),
                    //CompanyIntroduction = t.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[1].TextContent,
                })
                .FirstOrDefault();
            return detailsInfo;
        }

        /// <summary>
        /// 获取前程详细信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>

        public async Task<DetailsInfo> GetDetailsInfoByQC(string url)
        {
            using HttpClient http = new HttpClient();
            var htmlBytes = await http.GetByteArrayAsync(url);
            //【注意】使用GBK需要 Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册编码提供程序
            var htmlString = Encoding.GetEncoding("GBK").GetString(htmlBytes);
            HtmlParser htmlParser = new HtmlParser();
            var detailsInfo = htmlParser.ParseDocument(htmlString)
                .QuerySelectorAll(".tCompanyPage")
                .Where(t => t.QuerySelectorAll(".tBorderTop_box .t1 span").FirstOrDefault() != null)
                .Select(t => new DetailsInfo()
                {
                    //Experience = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[4].TextContent,
                    Education = t.QuerySelectorAll(".tBorderTop_box .t1 span")[0].TextContent,
                    CompanyNature = t.QuerySelectorAll(".msg.ltype")[0].TextContent,
                    //CompanySize = t.QuerySelectorAll(".terminalpage-right .terminal-company li")[0].TextContent,
                    Requirement = t.QuerySelectorAll(".bmsg.job_msg.inbox")[0].TextContent.Replace("职位描述：", ""),
                    CompanyIntroduction = t.QuerySelectorAll(".tmsg.inbox")[0].TextContent,
                })
                .FirstOrDefault();
            return detailsInfo;
        }

        /// <summary>
        /// 获取拉勾详细信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<DetailsInfo> GetDetailsInfoByLG(string url)
        {
            using HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0");
            var htmlString = await http.GetStringAsync(url);
            HtmlParser htmlParser = new HtmlParser();
            var detailsInfo = htmlParser.ParseDocument(htmlString)
                .QuerySelectorAll("body")
                .Select(t => new DetailsInfo()
                {
                    Experience = t.QuerySelectorAll(".job_request p").FirstOrDefault()?.TextContent,
                    //Education = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[5].TextContent,
                    CompanyNature = t.QuerySelectorAll(".job_company .c_feature li")?.Length <= 0 ? "" : t.QuerySelectorAll(".job_company .c_feature li")[0]?.TextContent,
                    CompanySize = t.QuerySelectorAll(".job_company .c_feature li")?.Length <= 2 ? "" : t.QuerySelectorAll(".job_company .c_feature li")[2]?.TextContent,
                    Requirement = t.QuerySelectorAll(".job_bt div").FirstOrDefault()?.TextContent.Replace("职位描述：", ""),
                    //CompanyIntroduction = t.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[1].TextContent,
                })
                .FirstOrDefault();
            return detailsInfo;
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="city"></param>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public EasyCache<List<JobInfo>> GetCacheObject(int? minutes = null)
        {
            var key = Request.Path.Value + Request.QueryString.Value;
            var time = DateTime.Now.AddMinutes(minutes ?? 10) - DateTime.Now;//缓存10分钟
            EasyCache<List<JobInfo>> obj = new EasyCache<List<JobInfo>>(key, time);
            return obj;
        }
    }
}
