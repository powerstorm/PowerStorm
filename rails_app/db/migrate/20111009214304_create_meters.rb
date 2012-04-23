class CreateMeters < ActiveRecord::Migration
  def self.up
    create_table :meters do |t|
      t.string :ip
      t.integer :building_id

      t.timestamps
    end
  end

  def self.down
    drop_table :meters
  end
end
