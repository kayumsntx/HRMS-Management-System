using HRMS.Application.DTOs.IdCards;

namespace HRMS.Application.Common.Interfaces;

public interface IIdCardPdfGenerator
{
    byte[] GeneratePdf(IdCardPdfModel model);   // 2 pages (front, back), CR80 card size
    byte[] GenerateQrPng(string text);
}
