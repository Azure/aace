import * as yup from "yup";

export const registerYupMethods = () => {
  yup.addMethod(yup.object, 'uniqueProperty', function (propertyName, message) {
    return this.test('unique', message, function (value) {
      if (!value || !value[propertyName] || (value['isDeleted'] && value['isDeleted'] === true)) {
        return true;
      }

      if (
        this.parent
          .filter(v => v !== value)
          .some(v => v[propertyName] === value[propertyName] && (v['isDeleted'] ? !!v['isDeleted'] === false : true))
      ) {
        throw this.createError({
          path: `${this.path}.${propertyName}`,
        });
      }

      return true;
    });
  });
}