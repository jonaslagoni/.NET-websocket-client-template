
namespace Blackhawk.Websocket.Messages.Structs
{
    public class {{ message.name() }}
    {
        {%- for propertyName, property in (message.payload() | getAllProperties()) %}
        public {{ property | getType() }} {{ propertyName }} { get; set; }
        {%- endfor %}
        public {{ message.name() }}(){
        }
        {%- if 0 != (message.payload().properties() | requiredProperties()) %}
        public {{ message.name() }}(
            {%- set counter = 1 %}
            {%- for propertyName, property in (message.payload() | getAllProperties()) %}
        {{ property | print }}
                {%- if property.required() %}
                    {%- if counter == message.payload().properties() | requiredProperties()  %}
            {{ property | getType() }} {{ propertyName }}
                        {%- else %}
            {{ property | getType() }} {{ propertyName }},
                        {%- endif %}
                    {%- set counter = counter + 1 %}
                {%- endif %}
            {%- endfor %}){
            {%- for propertyName, property in (message.payload() | getAllProperties()) %}
                {%- if property.required() %}
            this.{{ propertyName }}={{ propertyName }};
                {%- endif %}
            {%- endfor %}
        }
        {%- endif %}
    }
}
