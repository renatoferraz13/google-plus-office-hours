(function($) {
  $.fn.recentGooglePlusActivity = function(options) {

    var settings = $.extend({
      'maxResults' : '5'
    }, options);

    return this.each(function() {
      var $this = $(this);
      
      gapi.client.setApiKey(settings.apiKey);
      gapi.client.load('plus', 'v1', function() {
        var request = gapi.client.plus.activities.list({
          'userId': settings.userId,
          'maxResults': settings.maxResults,
          'collection': 'Public'
        });

        request.execute(function(response) {
          var activities = response.items;
          var activitiesHtml = "";
          for(var i in activities) {
            var when = activities[i].published;
            var url = activities[i].url;
            var activityContent = activities[i].object.content;

            activitiesHtml += "<li><a class='when' href='"+url+"'>" +
              when +"</a> " + activityContent;
          }
          $this.append("<ul>" + activitiesHtml + "</ul>");
        });
      });
    });
  };
})(jQuery);