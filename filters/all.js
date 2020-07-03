const URL = require('url');
const path = require('path');
const _ = require('lodash');
const filter = module.exports;

filter.kebabCase = string => {
	return _.kebabCase(string);
};

filter.camelCase = string => {
	return _.camelCase(string);
};

filter.firstLowerCase = string => {
	return _.lowerFirst(string);
};

filter.fileName = string => {
	return _.camelCase(string);
};

filter.oneLine = string => {
	if (!string) return string;
	return string.replace(/\n/g, ' ');
};
filter.print = property => {
	console.log(`[print] ${JSON.stringify(property, null, 4)}`);
};
filter.getAllProperties = payload => {
	if (!payload) {
		throw new Error('payload was not found for getAllProperties');
	}
	console.log(
		`[getAllProperties] - finding properties for: ${JSON.stringify(
			payload,
			null,
			4
		)}`
	);
	let properties = {};
	try {
		if (payload._json.type) {
			for (var propertyIndex in payload._json.properties) {
				let property = payload._json.properties[propertyIndex];
				properties[propertyIndex] = property;
			}
		} else {
			if (payload._json.allOf) {
				for (index in payload._json.allOf) {
					let value = payload._json.allOf[index];
					for (var propertyIndex in value.properties) {
						properties[propertyIndex] = value.properties[propertyIndex];
					}
				}
			}
		}
		console.log(
			`[getAllProperties] - Returning ${JSON.stringify(properties, null, 4)}`
		);
		return properties;
	} catch (e) {
		throw new Error('Error occured in getAllProperties ' + e);
	}
};

filter.getType = property => {
	if (!property) {
		throw new Error('Property was not found for getType');
	}
	let type = '';
	console.log(
		`[getType] - finding type for property: ${JSON.stringify(
			property,
			null,
			4
		)}`
	);
	try {
		if (property._json == null) {
			//value is from getAllProperties
			type = property.type ? property.type : 'MISSING REF!? ';
		} else if (!property._json.type) {
			type = property._json.name ? property._json.name : 'MISSING REF!? ';
		} else {
			type = property._json.type;
		}
		console.log(`[getType] - Returning type: ${type}`);
		return type;
	} catch (e) {
		throw new Error('Error occured in getType ' + e);
	}
};

filter.requiredProperties = array => {
	if (!array) {
		throw new Error('Array was not found for requiredProperties');
	}
	try {
		let counter = 0;
		for (let index in array) {
			let value = array[index];
			if (value.required == true || value._json.required == true) {
				counter++;
			}
		}
		return counter;
	} catch (e) {
		throw new Error(
			'Error occured while trying to find requiredProperties: ' + e
		);
	}
};

filter.containsTag = (array, tag) => {
	if (!array || !tag) {
		return false;
	}
	return array.find(value => {
		return tag === value.name();
	});
};

filter.docline = (field, fieldName, scopePropName) => {
	const buildLine = (f, fName, pName) => {
		const type = f.type() ? f.type() : 'string';
		const description = f.description()
			? ` - ${f.description().replace(/\r?\n|\r/g, '')}`
			: '';
		let def = f.default();

		if (def && type === 'string') def = `'${def}'`;

		let line;
		if (def !== undefined) {
			line = ` * @param {${type}} [${
				pName ? `${pName}.` : ''
			}${fName}=${def}]`;
		} else {
			line = ` * @param {${type}} ${pName ? `${pName}.` : ''}${fName}`;
		}

		if (type === 'object') {
			let lines = `${line}\n`;
			let first = true;
			for (const propName in f.properties()) {
				lines = `${lines}${first ? '' : '\n'}${buildLine(
					f.properties()[propName],
					propName,
					`${pName ? `${pName}.` : ''}${fName}`
				)}`;
				first = false;
			}
			return lines;
		}

		return `${line}${description}`;
	};

	return buildLine(field, fieldName, scopePropName);
};

filter.port = (url, defaultPort) => {
	const parsed = URL.parse(url);
	return parsed.port || defaultPort || 80;
};

filter.pathResolve = (pathName, basePath = '/') => {
	return path.resolve(basePath, pathName);
};

filter.throw = message => {
	throw new Error(message);
};