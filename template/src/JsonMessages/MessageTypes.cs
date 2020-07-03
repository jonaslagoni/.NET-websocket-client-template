
namespace Blackhawk.Websocket.Messages
{
	public enum MessageTypes
	{
		subscribe,
        {% set counter = 0 %}
        {% for channelName, channel in asyncapi.channels() %}
        {% if channel.hasPublish() %}
        {% if channel.publish().tags() | length == 0 or channel.publish().tags() | containsTag("client") %}
        {% if counter+1 == asyncapi.channels() | length %}
        {{ channelName | camelCase }}
        {%- else %}
        {{ channelName | camelCase }},
        {%- endif %}
        {%- endif %}
        {%- endif %}
        {%- endfor %}
    };
}
