using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CrossExchange.Controller
{
    [Route("api/Trade")]
    public class TradeController : ControllerBase
    {
        private IShareRepository _shareRepository { get; set; }
        private ITradeRepository _tradeRepository { get; set; }
        private IPortfolioRepository _portfolioRepository { get; set; }

        public TradeController(IShareRepository shareRepository, ITradeRepository tradeRepository, IPortfolioRepository portfolioRepository)
        {
            _shareRepository = shareRepository;
            _tradeRepository = tradeRepository;
            _portfolioRepository = portfolioRepository;
        }


        [HttpGet("{portfolioid}")]
        public async Task<IActionResult> GetAllTradings([FromRoute]int portFolioid)
        {
            var trade = _tradeRepository.Query().Where(x => x.PortfolioId.Equals(portFolioid));
            return Ok(trade);
        }



        /*************************************************************************************************************************************
        For a given portfolio, with all the registered shares you need to do a trade which could be either a BUY or SELL trade. 
        For a particular trade keep following conditions in mind:
		BUY:
        a) The rate at which the shares will be bought will be the latest price in the database.
		b) The share specified should be a registered one otherwise it should be considered a bad request. 
		c) The Portfolio of the user should also be registered otherwise it should be considered a bad request. 
                
        SELL:
        a) The share should be there in the portfolio of the customer.
		b) The Portfolio of the user should be registered otherwise it should be considered a bad request. 
		c) The rate at which the shares will be sold will be the latest price in the database.
        d) The number of shares should be sufficient so that it can be sold. 
        Hint: You need to group the total shares bought and sold of a particular share and see the difference to figure out if 
        there are sufficient quantities available for SELL. 

        *************************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TradeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var portfolio = _portfolioRepository.Query().Where(x => x.Id == model.PortfolioId).ToList();
            if (portfolio == null || portfolio.Count == 0)
            { return BadRequest(ModelState); }
            var latestPrice = _shareRepository.Query().Where(x => x.Symbol.Equals(model.Symbol)).OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            if (latestPrice == null)
            { return BadRequest(ModelState); }
            Trade tradeObj = null;
            if (model.Action == "BUY")
            {
                tradeObj = new Trade();
                tradeObj.PortfolioId = model.PortfolioId;
                tradeObj.Symbol = model.Symbol;
                tradeObj.NoOfShares = model.NoOfShares;
                tradeObj.Action = model.Action;
                tradeObj.Price = model.NoOfShares * latestPrice.Rate;
            }

            if (model.Action == "SELL")
            {
                var shareDetails = _tradeRepository.Query().Where(x => x.PortfolioId == model.PortfolioId && x.Symbol == model.Symbol).ToList();
                if (shareDetails == null || shareDetails.Count == 0)
                { return BadRequest(ModelState); }
                var totalBought = shareDetails.Where(x => x.Action == "BUY").Sum(x => x.NoOfShares);
                var totalSold = shareDetails.Where(x => x.Action == "SELL").Sum(x => x.NoOfShares);
                if (totalBought - totalSold >= model.NoOfShares)
                {
                    tradeObj = new Trade();
                    tradeObj.PortfolioId = model.PortfolioId;
                    tradeObj.Symbol = model.Symbol;
                    tradeObj.NoOfShares = model.NoOfShares;
                    tradeObj.Action = model.Action;
                    tradeObj.Price = model.NoOfShares * latestPrice.Rate;
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            await _tradeRepository.InsertAsync(tradeObj);
            return Created("Trade", tradeObj);
        }

    }
}
