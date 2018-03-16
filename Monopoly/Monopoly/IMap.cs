using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace Monopoly
{
    /* @interface IMap
     * 
     * @brief This interface should have implementations for each OS platform to get
     * essential GPS information required for this application
     * 
     */
    public interface IMap
    {
        // @returns a (double, double) representing the device's current GPS coordinates
        Task<(double, double)> GetCurrentCoordinates();

        /* An event that that will notify all subscribers that the current location 
         * has changed.
         * 
         * @param (double, double)          GPS coordinates of the new location
         */
        event EventHandler<(double, double)> LocationChanged;
    }
}
